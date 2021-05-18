using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgorithm : MonoBehaviour
{
    public bool writeFitnessToFile;
    public int populationSize;
    public float mutationRate;
    public float mutationAmount;
    public float crossOverRate;
    public float elitism;
    public int iterationsBeforeNewPopulation;
    public int[] networkLayers;

    private NeuralNetwork[] population;
    private float[] fitness;

    private NeuralNetwork best;
    private float bestFitness = Mathf.NegativeInfinity;
    private int rankSum; // for rank selection

    BirdGameManager gameManager;


    System.IO.StreamWriter file;

    System.Random random = new System.Random(123);

    private int currentIteration = 0;

    // Use this for initialization
    void Start()
    {
        gameManager = GameObject.Find("_SCRIPTS").GetComponent<BirdGameManager>();
        if (writeFitnessToFile)
        {
            file = new System.IO.StreamWriter(@"C:\Users\Jure\Desktop\fitness.txt");
        }

        initPopulation();
    }

    private void initPopulation()
    {
        population = new NeuralNetwork[populationSize];
        fitness = new float[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            rankSum += i + 1;
            population[i] = new NeuralNetwork(networkLayers);
        }
    }


    private NeuralNetwork mutate(NeuralNetwork nn)
    {
        Matrix<double>[] weights = nn.GetWeights();
        foreach (Matrix<double> m in weights)
        {
            Matrix<double> r = Matrix.Round(Matrix.Build.Random(m.RowCount, m.ColumnCount, new ContinuousUniform(0.5 * mutationRate, 0.5 + 0.5 * mutationRate)));
            r = r.PointwiseMultiply(Matrix.Build.Random(m.RowCount, m.ColumnCount, new Normal(0, mutationAmount)));
            m.Add(r, m);
        }
        return nn;
    }
    /*
    private NeuralNetwork crossOver(NeuralNetwork nn1, NeuralNetwork nn2)
    {
        Matrix<double>[] weights1 = nn1.GetWeights();
        Matrix<double>[] weights2 = nn2.GetWeights();

        NeuralNetwork nn = new NeuralNetwork(networkLayers);

        for (int i = 0; i < weights1.Length; i++)
        {
            Matrix<double> m1 = weights1[i];
            Matrix<double> m2 = weights2[i];
            if (m1.RowCount <= 1)
            {
                continue;
            }

            int randomRow = random.Next(0, m1.RowCount - 1) + 1;
            Matrix<double> m = m1.SubMatrix(0, randomRow, 0, m1.ColumnCount).Stack(m2.SubMatrix(randomRow, m2.RowCount - randomRow, 0, m2.ColumnCount));
            nn.SetWeights(i, m);
        }
        return nn;
    }
    */

    private NeuralNetwork crossOver(NeuralNetwork nn1, NeuralNetwork nn2)
    {
        Matrix<double>[] weights1 = nn1.GetWeights();
        Matrix<double>[] weights2 = nn2.GetWeights();

        Matrix<double>[] bias1 = nn1.GetBias();
        Matrix<double>[] bias2 = nn2.GetBias();

        NeuralNetwork nn = new NeuralNetwork(networkLayers);

        for (int i = 0; i < weights1.Length; i++)
        {
            if (random.NextDouble() < crossOverRate)
            {
                nn.SetWeights(i, weights2[i]);
                nn.SetBias(i + 1, bias2[i + 1]);
            }
            else
            {
                nn.SetWeights(i, weights1[i]);
                nn.SetBias(i + 1, bias1[i + 1]);
            }
        }
        return nn;
    }

    private NeuralNetwork pickRoulette(float sum)
    {
        double r = random.NextDouble();
        int i = 0;
        while (r >= 0)
        {
            r -= fitness[i] / sum;
            if (r < 0)
            {
                break;
            }
            i++;
        }
        return population[i].Copy();
    }

    private NeuralNetwork pickRank()
    {
        double r = random.NextDouble();
        int i = 0;
        while (r >= 0)
        {
            r -= (populationSize - i) / rankSum;
            if (r < 0)
            {
                break;
            }
            if (i == populationSize - 1)
                break;
            i++;
        }
        return population[i].Copy();
    }

    private void bestFitnessCalculation()
    {
        float sum = 0;  // Also use for roulette selection
        float currentBestFitness = Mathf.NegativeInfinity;
        for (int i = 0; i < populationSize; i++)
        {
            if (fitness[i] >= bestFitness)
            {
                bestFitness = fitness[i];
                best = population[i];
            }
            if (fitness[i] >= currentBestFitness)
            {
                currentBestFitness = fitness[i];
            }
            sum += fitness[i];
        }
        //Debug.Log("Best fitness was " + currentBestFitness);
        if (writeFitnessToFile)
        {
            file.WriteLine("" + currentBestFitness);
            file.Flush();
        }
        // save the best neural network
        if (best != null)
        {
            Debug.Log("Saving agent to file. (Fitness = " + currentBestFitness + ")");
            best.saveToFile("bestAgent.ga");
        }
    }

    public void Reset()
    {
        currentIteration += 1;
        if (currentIteration >= iterationsBeforeNewPopulation)
        {
            currentIteration = 0;
            NeuralNetwork[] nextPopulation = new NeuralNetwork[populationSize];
            bestFitnessCalculation();

            Array.Sort(fitness, population);
            Array.Reverse(population);

            // Create the new population

            // Copy over the elite
            int eliteSize = (int)(elitism * populationSize);
            for (int i = 0; i < eliteSize; i++)
            {
                nextPopulation[i] = population[i].Copy();
            }

            // Fill the rest of the population
            for (int i = eliteSize; i < nextPopulation.Length; i++)
            {
                NeuralNetwork n1 = pickRank();
                NeuralNetwork n = n1;
                if (random.NextDouble() < crossOverRate)
                {
                    NeuralNetwork n2 = pickRank();
                    n = crossOver(n1, n2);

                    if (random.NextDouble() < mutationRate)
                    {
                        n = mutate(n);
                    }
                }
                nextPopulation[i] = n;
            }

            population = nextPopulation;
            fitness = new float[populationSize];
        }
        if (gameManager != null)
            gameManager.Reset();
    }

    public void setFitness(int id, float value)
    {
        fitness[id] = value;
    }

    public void addFitness(int id, float value)
    {
        fitness[id] += value;
        if(fitness[id] < 0)
        {
            fitness[id] = 0;
        }
    }

    public double[] GetPrediction(int id, double[] input)
    {
        Matrix<double> result = population[id].feed(DenseVector.OfArray(input).ToColumnMatrix());
        return result.ToColumnArrays()[0];
    }

    public double[] GetPrediction(NeuralNetwork network, double[] input)
    {
        Matrix<double> result = network.feed(DenseVector.OfArray(input).ToColumnMatrix());
        return result.ToColumnArrays()[0];
    }

    public int getInputSize()
    {
        return networkLayers[0];
    }
}
