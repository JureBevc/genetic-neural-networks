using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.IO;
using UnityEngine;

public class NeuralNetwork
{

    private System.Random random = new System.Random(123);

    private int[] layers;
    private Matrix<double>[] nodes;
    private Matrix<double>[] weights;
    private Matrix<double>[] bias;


    public NeuralNetwork(int[] layers)
    {
        this.layers = layers;

        nodes = new Matrix<double>[layers.Length];
        weights = new Matrix<double>[layers.Length - 1];
        bias = new Matrix<double>[layers.Length];

        // Initialize layers with random values from normal distribution
        for (int i = 0; i < layers.Length; i++)
        {
            nodes[i] = DenseMatrix.Build.Random(layers[i], 1);
            bias[i] = DenseMatrix.Build.Random(layers[i], 1);
            if (i < layers.Length - 1)
            {
                weights[i] = DenseMatrix.Build.Random(layers[i + 1], layers[i]);
            }
        }
    }

    public Matrix<double> feed(Matrix<double> input)
    {
        if (layers[0] != input.RowCount)
        {
            Debug.LogError("Input size does not match number of input nodes!");
            Debug.LogError("Given " + input.RowCount + ", expected " + layers[0]);
            return nodes[layers.Length - 1];
        }

        if (1 != input.ColumnCount)
        {
            Debug.LogError("Input size does not have only one column!");
            Debug.LogError("Given " + input.ColumnCount + ", expected " + 1);
            return nodes[layers.Length - 1];
        }

        nodes[0] = input;
        for (int i = 1; i < layers.Length; i++)
        {
            nodes[i] = act(weights[i - 1] * nodes[i - 1] + bias[i]);
        }

        return nodes[layers.Length - 1];
    }

    public NeuralNetwork Copy()
    {
        NeuralNetwork n = new NeuralNetwork(layers);
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i].CopyTo(n.weights[i]);
        }
        for (int i = 0; i < bias.Length; i++)
        {
            bias[i].CopyTo(n.bias[i]);
        }
        return n;
    }

    private Matrix<double> act(Matrix<double> m)
    {
        return tanh(m);
    }

    // Activation functions
    private Matrix<double> sigmoid(Matrix<double> m)
    {
        return 1 / (1 + Matrix.Exp(-m));
    }

    private Matrix<double> tanh(Matrix<double> m)
    {
        return Matrix.Tanh(m);
    }

    private Matrix<double> relu(Matrix<double> m)
    {
        return m.PointwiseMaximum(0);
    }

    private Matrix<double> leaky_relu(Matrix<double> m)
    {
        return m.PointwiseMaximum(0.1 * m);
    }


    public Matrix<double>[] GetWeights()
    {
        return weights;
    }

    public Matrix<double>[] GetBias()
    {
        return bias;
    }

    public void SetWeights(int i, Matrix<double> m)
    {
        weights[i] = m;
    }

    public void SetBias(int i, Matrix<double> m)
    {
        bias[i] = m;
    }

    public void saveToFile(string fileName)
    {
        using (TextWriter tw = new StreamWriter(fileName))
        {
            for (int w = 0; w < weights.Length; w++)
            {
                Matrix<double> matrix = weights[w];
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    for (int i = 0; i < matrix.RowCount; i++)
                    {
                        if (i != 0)
                        {
                            tw.Write(" ");
                        }
                        tw.Write(matrix[i, j]);
                    }
                    tw.WriteLine();
                }
                if (w + 1 < weights.Length)
                {
                    tw.Write("+");
                    tw.WriteLine();
                }
            }
            tw.Write("b");
            tw.WriteLine();
            for (int w = 0; w < bias.Length; w++)
            {
                Matrix<double> matrix = bias[w];
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    for (int i = 0; i < matrix.RowCount; i++)
                    {
                        if (i != 0)
                        {
                            tw.Write(" ");
                        }
                        tw.Write(matrix[i, j]);
                    }
                    tw.WriteLine();
                }
                if (w + 1 < bias.Length)
                {
                    tw.Write("+");
                    tw.WriteLine();
                }
            }


        }
    }

    public void loadFromFile(string fileName)
    {
        string all = File.ReadAllText(fileName);
        string[] wb = all.Split(new string[] { "b\r\n" }, StringSplitOptions.None);
        string[] w = wb[0].Split(new string[] { "+\r\n" }, StringSplitOptions.None);
        string[] b = wb[1].Split(new string[] { "+\r\n" }, StringSplitOptions.None);
        for(int wi = 0; wi < w.Length; wi++) 
        {
            Matrix<double> weight = weights[wi];
            string[] rows = w[wi].Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for(int i = 0; i < weight.ColumnCount; i++) 
            {
                string[] row = rows[i].Split(' '); 
                for(int j = 0; j < weight.RowCount; j++)
                {
                    weight[j, i] = double.Parse(row[j]);
                }
            }
        }

        for (int bi = 0; bi < b.Length; bi++)
        {
            Matrix<double> biasMatrix = bias[bi];
            string[] rows = b[bi].Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < biasMatrix.ColumnCount; i++)
            {
                string[] row = rows[i].Split(' ');
                for (int j = 0; j < biasMatrix.RowCount; j++)
                {
                    biasMatrix[j, i] = double.Parse(row[j]);
                }
            }
        }
    }
}
