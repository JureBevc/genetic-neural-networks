using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdAgent : Agent
{


    BirdGameManager gameManager;
    BirdControl birdControl;
    
    public void Start()
    {
        gameManager = GameObject.Find("_SCRIPTS").GetComponent<BirdGameManager>();
        if (!gameManager.useGA)
        {
            //birdControl = gameManager.GetBird().GetComponent<BirdControl>();
        }
    }

    public void setBirdControl(BirdControl birdControl)
    {
        this.birdControl = birdControl;
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);
        //Debug.Log(vectorAction[0]);
        if (vectorAction[0] == 1)
        {
            birdControl.Jump();
        }
        AddReward(0.01f);
    }

    public override void AgentReset()
    {
        base.AgentReset();
        gameManager.Reset();
    }

    public override void CollectObservations()
    {
        base.CollectObservations();
        if (gameManager.useCamera)
        {
            return;
        }
        float maxWidth = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        float maxHeight = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;

        GameObject bottomPipe = gameManager.GetBottomPipe();
        GameObject topPipe = gameManager.GetTopPipe();

        float bottom = (bottomPipe == null) ? maxHeight : bottomPipe.transform.position.y;
        float top = (topPipe == null) ? Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y : topPipe.transform.position.y;
        float distance = (bottomPipe == null) ? Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x : bottomPipe.transform.position.x;

        GameObject bird = gameManager.GetBird(birdControl.GetIndex());
        float birdX = bird.transform.position.x;
        float birdY = bird.transform.position.y;

        distance = distance - birdX;

        AddVectorObs(bottom / maxHeight);
        AddVectorObs(top / maxHeight);
        AddVectorObs(distance / maxWidth);
        AddVectorObs(birdY / maxHeight);
    }

    public override void AgentOnDone()
    {
        base.AgentOnDone();
        Debug.Log(GetReward());
    }

    public void BirdFailed()
    {
        AddReward(-1f);
        Done();
    }

    public void BirdOffScreen()
    {
        AddReward(-1f);
        Done();
    }

    public void BirdSuccess()
    {
        //AddReward(10f);
    }
}