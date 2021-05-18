using MLAgents;
using System;
using UnityEngine;

public class TetrisAgent : Agent
{

    TetrisGameManager gameManager;

    public void setGameManager(TetrisGameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);

        int[] prediciton = new int[] { (int) vectorAction[0], (int) vectorAction[1]};
        Debug.Log(prediciton[0]);
         if (prediciton[0] == 1)
        {
            gameManager.flip();
        }
        else if (prediciton[0] == 2)
        {
            gameManager.flip();
            gameManager.flip();
        }
        else if(prediciton[0] == 3)
        {
            gameManager.flip();
            gameManager.flip();
            gameManager.flip();
        }
        int moveAmount = Mathf.RoundToInt(prediciton[1] + 1 - gameManager.getGridWidth() / 2);
        for (int j = 0; j < Mathf.Abs(moveAmount); j++)
        {
            if (moveAmount > 0)
            {
                gameManager.moveRight();
            }
            else
            {
                gameManager.moveLeft();
            }
        }
        gameManager.placeDown();
    }

    public override void AgentReset()
    {
        base.AgentReset();
    }

    public override void CollectObservations()
    {
        base.CollectObservations();

        int[,] data = gameManager.getData();
        for(int x = 0; x < data.GetLength(0); x++)
        {
            for (int y = 0; y < data.GetLength(1); y++)
            {
                AddVectorObs(data[x,y]);
            }
        }
    }

    public override void AgentOnDone()
    {
        base.AgentOnDone();
    }

}
