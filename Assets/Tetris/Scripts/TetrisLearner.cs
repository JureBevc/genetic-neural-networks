using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisLearner : MonoBehaviour
{

    public bool useGA;
    public float updateFrequency;

    public bool updateGrid;

    GeneticAlgorithm GA;

    GameObject gridObject;

    TetrisGameManager manager;
    TetrisAgent agent;


    TetrisGameManager[] managers;
    // Use this for initialization
    void Start()
    {
        gridObject = Resources.Load<GameObject>("Tetris/Grid");


        if (useGA)
        {

            GameObject.Find("Academy").SetActive(false);
            GA = GetComponent<GeneticAlgorithm>();
            managers = new TetrisGameManager[GA.populationSize];
            for (int i = 0; i < managers.Length; i++)
            {
                GameObject grid = Instantiate(gridObject, new Vector3((i % 5) * 6, -(i / 5) * 12), Quaternion.identity);
                managers[i] = grid.GetComponent<TetrisGameManager>();
            }
        }
        else
        {
            GameObject grid = Instantiate(gridObject);
            manager = grid.GetComponent<TetrisGameManager>();
            agent = grid.transform.Find("TetrisAgent").GetComponent<TetrisAgent>();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = updateFrequency;
        if (useGA)
        {
            int gameOvers = 0;
            for (int i = 0; i < GA.populationSize; i++)
            {
                if (managers[i].isGameOver())
                {
                    gameOvers++;
                }
                else
                {
                    // Input, prediction, action
                    int[,] data = managers[i].getData();
                    double[] input = new double[GA.getInputSize()];
                    for (int x = 0; x < data.GetLength(0); x++)
                    {
                        for (int y = 0; y < data.GetLength(1); y++)
                        {
                            input[x + y * data.GetLength(0)] = data[x, y];
                        }
                    }
                    double[] prediciton = GA.GetPrediction(i, input);

                    if (prediciton[0] < 0.25f)
                    {

                    }
                    else if (prediciton[0] < 0.5f)
                    {
                        managers[i].flip();
                    }
                    else if (prediciton[0] < 0.75f)
                    {
                        managers[i].flip();
                        managers[i].flip();
                    }
                    else
                    {
                        managers[i].flip();
                        managers[i].flip();
                        managers[i].flip();
                    }
                    int moveAmount = Mathf.RoundToInt((float)prediciton[1] * managers[i].getGridWidth());
                    for(int j = 0; j < Mathf.Abs(moveAmount); j++)
                    {
                        if(moveAmount > 0)
                        {
                            managers[i].moveRight();
                        }
                        else
                        {
                            managers[i].moveLeft();
                        }
                    }
                    managers[i].placeDown();
                    
                    // Update game
                    bool drawGrid = updateGrid && (i < 10);
                    managers[i].updateGame(drawGrid);

                    // Rewards
                    if (managers[i].isLineFilled())
                    {
                        //GA.addFitness(i, 1f);
                    }
                    if (managers[i].isPlaced())
                    {
                        //GA.addFitness(i, 0.01f * (1 - managers[i].getPlacedHeight() * 1.0f / managers[i].getGridHeight()));
                        GA.addFitness(i, 0.01f);
                        //GA.addFitness(i, 0.05f * (1.0f - managers[i].getBumpCount() / managers[i].getGridSize()));
                    }

                    if (managers[i].isGameOver())
                    {
                        GA.addFitness(i, managers[i].getScore() * 0.5f);
                        //GA.addFitness(i, (1 - (managers[i].getHoleCount() * 1.0f / managers[i].getGridSize())));
                    }
                }
            }
            if (gameOvers == GA.populationSize)
            {
                GA.Reset();
                for (int i = 0; i < GA.populationSize; i++)
                {
                    managers[i].ResetGame();
                }
            }
        }
        else
        {
            if (manager.isGameOver())
            {
                manager.getAgent().Done();
                manager.ResetGame();
            }
            else
            {
                manager.updateGame(updateGrid);
                if (manager.isPlaced())
                {
                    agent.AddReward(0.01f);
                    agent.AddReward(-(manager.getHoleCount(manager.getGrid()) * 1.0f / manager.getGridSize()));
                    agent.AddReward(- manager.getBumpCount(manager.getGrid()) / manager.getGridSize());
                }
            }
        }
    }
}
