
using UnityEngine;
using UnityEngine.UI;

public class BirdGameManager : MonoBehaviour
{
    [Header("Genetic algorithm settings")]
    public bool useGA;
    public bool loadFromFile;
    public float incremental;
    public int maxScore;


    [Header("ML-Agents settings")]
    public bool useCamera;
    public int numberOfAgents;
    public float randomPositionAmount;

    [Header("Game speed")]
    public float updateFrequency;

    [Header("Game settings")]
    public Vector2 birdStartingPosition;
    public float pipeSpawnFrequency;
    public float pipeGapSize;
    public float pipeGapDeviation;
    public float pipeSpeed;

    System.IO.StreamWriter file;

    // GA
    GameObject birdParent;
    GameObject[] birds;
    BirdControl[] birdControls;
    float fitness = 0;
    int incrementalScore = 1;
    NeuralNetwork loadedNetwork;
    BirdControl birdControl;

    GameObject pipes;
    float pipeSpawnTime = 0;

    bool firstNext = true;
    GameObject nextTop, nextBottom;

    int score = 0;
    Text scoreText;
    Text learningScoreText;

    // used for ML Agents
    BirdAgent birdAgent;
    GameObject bird; // One bird for ML Agents
    BirdAgent[] birdAgents;

    GeneticAlgorithm GA;

    System.Diagnostics.Stopwatch executionTime;
    // Use this for initialization
    void Awake()
    {
        pipes = GameObject.Find("Pipes");
        scoreText = GameObject.Find("Canvas/Text").GetComponent<Text>();
        learningScoreText = GameObject.Find("Canvas/LearningScore").GetComponent<Text>();
        if (!useGA)
        {
            GameObject birdObject = Resources.Load<GameObject>("Bird/Bird");
            //file = new System.IO.StreamWriter(@"C:\Users\Jure\Desktop\ml-agents-fitness.txt");
            GameObject.Find("Academy").GetComponent<BirdAcademy>().enabled = true;
            /*
            bird = Instantiate(birdObject, new Vector3(birdStartingPosition.x, birdStartingPosition.y), Quaternion.identity);
            bird.name = "Bird";
            birdAgent = bird.GetComponent<BirdAgent>();
            birdAgent.Start();
            */

            birdParent = Instantiate(new GameObject());
            birdParent.name = "Birds";
            birds = new GameObject[numberOfAgents];
            birdAgents = new BirdAgent[numberOfAgents];
            birdControls = new BirdControl[numberOfAgents];
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i] = Instantiate(birdObject, new Vector3(birdStartingPosition.x, birdStartingPosition.y + Random.Range(-1.0f, 1.0f) * randomPositionAmount), Quaternion.identity);
                birds[i].name = "Bird";
                birdControls[i] = birds[i].GetComponent<BirdControl>();
                birdControls[i].SetIndex(i);
                birdAgents[i] = birds[i].GetComponent<BirdAgent>();
                birdAgents[i].setBirdControl(birdControls[i]);
                birds[i].transform.SetParent(birdParent.transform);
            }
            foreach (BirdAgent ba in birdAgents)
            {
                ba.Start();
            }
        }
        else if (!loadFromFile)
        {
            GameObject.Find("Academy").GetComponent<BirdAcademy>().enabled = false;
            GA = GameObject.Find("_SCRIPTS").GetComponent<GeneticAlgorithm>();


            GameObject birdObject = Resources.Load<GameObject>("Bird/Bird");
            birdParent = Instantiate(new GameObject());
            birdParent.name = "Birds";

            birds = new GameObject[GA.populationSize];
            birdControls = new BirdControl[GA.populationSize];
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i] = Instantiate(birdObject, new Vector3(birdStartingPosition.x, birdStartingPosition.y), Quaternion.identity);
                birds[i].GetComponent<BirdAgent>().enabled = false;
                birds[i].name = "Bird" + i;
                birds[i].transform.SetParent(birdParent.transform);
                birdControls[i] = birds[i].GetComponent<BirdControl>();
                birdControls[i].SetIndex(i);
            }
        }
        else
        {
            GameObject.Find("Academy").GetComponent<BirdAcademy>().enabled = false;
            GA = GameObject.Find("_SCRIPTS").GetComponent<GeneticAlgorithm>();

            bird = Instantiate(Resources.Load<GameObject>("Bird/Bird"), new Vector3(birdStartingPosition.x, birdStartingPosition.y), Quaternion.identity);
            bird.name = "Bird";
            birdControl = bird.GetComponent<BirdControl>();
            loadedNetwork = new NeuralNetwork(GA.networkLayers);
            loadedNetwork.loadFromFile("bestAgent.ga");
        }
        executionTime = System.Diagnostics.Stopwatch.StartNew();
    }

    private void FixedUpdate()
    {
        Time.timeScale = updateFrequency;
        UpdateLogic();
    }

    private void UpdateLogic()
    {
        SetNextPipes();
        // spawn pipes
        pipeSpawnTime += Time.fixedDeltaTime;
        if (pipeSpawnTime >= (1f / pipeSpawnFrequency))
        {
            float center = Random.Range(-pipeGapDeviation, pipeGapDeviation);
            float topY = center + pipeGapSize / 2;
            float bottomY = center - pipeGapSize / 2;
            float x = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)).x + 1f;

            GameObject pipeTop = Instantiate(Resources.Load<GameObject>("Bird/Pipe"), new Vector3(x, topY), Quaternion.Euler(0, 180, 180));
            GameObject pipeBottom = Instantiate(Resources.Load<GameObject>("Bird/Pipe"), new Vector3(x, bottomY), Quaternion.Euler(0, 0, 0));

            pipeTop.transform.SetParent(pipes.transform);
            pipeBottom.transform.SetParent(pipes.transform);
            pipeTop.GetComponent<PipeMovement>().SetSpeed(-pipeSpeed);
            pipeBottom.GetComponent<PipeMovement>().SetSpeed(-pipeSpeed);

            pipeSpawnTime = 0;
        }

        if (!useGA)
        {
            // check if bird is off screen
            /*
            if (IsOffScreen(bird.transform.position))
            {
                birdAgent.BirdOffScreen();
            }
            fitness += 0.1f;
            */
            for (int i = 0; i < numberOfAgents; i++)
            {
                //Debug.Log("BIRDS: " + birds.Length);
                //Debug.Log("BIRDS NULL: " + (birds[i] == null));
                if (IsOffScreen(birds[i].transform.position))
                {
                    birdAgents[i].BirdOffScreen();
                }
            }
            fitness += 0.1f;
        }
        else
        {
            fitness += 0.1f;
            learningScoreText.text = "" + fitness;

            float maxWidth = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            float maxHeight = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;

            GameObject bottomPipe = GetBottomPipe();
            GameObject topPipe = GetTopPipe();

            float bottom = (bottomPipe == null) ? maxHeight : bottomPipe.transform.position.y;
            float top = (topPipe == null) ? Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y : topPipe.transform.position.y;
            float distance = (bottomPipe == null) ? Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x : bottomPipe.transform.position.x;

            float birdX = birdStartingPosition.x;

            distance = distance + 0.5f - birdX;

            double input1 = bottom / maxHeight;
            double input2 = top / maxHeight;
            //double input1 = ((top-bottom)/2) / maxHeight;
            double input3 = distance / maxWidth;

            if (loadFromFile)
            {
                float birdY = bird.transform.position.y;
                double input4 = birdY / maxHeight;
                double prediciton = GA.GetPrediction(loadedNetwork, new double[] { input1, input2, input3, input4 })[0];
                if (prediciton >= 0.5)
                {
                    birdControl.Jump();
                }
            }
            else
            {
                int alive = 0;
                for (int i = 0; i < GA.populationSize; i++)
                {
                    if (!birdControls[i].GetDead())
                    {
                        alive += 1;
                        float birdY = birds[i].transform.position.y;
                        double input4 = birdY / maxHeight;
                        double prediciton = GA.GetPrediction(i, new double[] { input1, input2, input3, input4 })[0];
                        if (prediciton >= 0.5)
                        {
                            birdControls[i].Jump();
                        }
                        if (IsOffScreen(birds[i].transform.position))
                        {
                            BirdFailed(i);
                        }
                    }
                }
                if (alive == 0)
                {
                    GA.Reset();
                }
                if (incremental > 0 && score == incrementalScore)
                {
                    for (int i = 0; i < GA.populationSize; i++)
                    {
                        if (!birdControls[i].GetDead())
                        {
                            GA.addFitness(i, fitness);
                        }
                    }

                    float aliveRatio = (float)alive / GA.populationSize;
                    Debug.Log((aliveRatio * 100) + "% survived.");
                    if (aliveRatio > incremental)
                    {
                        incrementalScore += 1;
                        Debug.Log("Next incremental score: " + incrementalScore);
                    }
                    GA.Reset();
                }
            }
        }
    }

    private bool IsOffScreen(Vector3 position)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(position);
        if (screenPosition.y > Screen.height || screenPosition.y < 0)
        {
            return true;
        }
        return false;
    }

    /*
     * Finds the first pipes in front of player
     */
    private void SetNextPipes()
    {
        GameObject previous = nextBottom;
        if (nextBottom != null && nextBottom.transform.position.x < birdStartingPosition.x)
        {
            nextBottom = null;
        }
        if (nextTop != null && nextTop.transform.position.x < birdStartingPosition.x)
        {
            nextTop = null;
        }
        foreach (Transform child in pipes.transform)
        {
            if (child.position.x + 0.5f > birdStartingPosition.x && (nextBottom == null || nextBottom.transform.position.x > child.position.x))
            {
                if (child.rotation.eulerAngles.z == 0)
                {
                    nextBottom = child.gameObject;
                }
                else
                {
                    nextTop = child.gameObject;
                }
            }
        }
        if (previous != nextBottom)
        {
            if (!firstNext)
            {
                if (!useGA)
                {
                    foreach (BirdAgent ba in birdAgents)
                    {
                        ba.BirdSuccess();
                    }
                }
                score += 1;
                scoreText.text = "" + score;
                if (score >= maxScore && maxScore != 0)
                {
                    executionTime.Stop();
                    Debug.Log("Agent reached " + maxScore + " points in " + (executionTime.ElapsedMilliseconds / 1000) + " seconds.");
                    Application.Quit();
                    #if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
                    #endif
                }
            }
            firstNext = false;
        }
    }

    public void BirdFailed(int index)
    {
        if (!useGA)
        {
            birdAgents[index].BirdFailed();
        }
        else
        {
            if (loadFromFile)
            {
                Reset();
                return;
            }
            birds[index].GetComponent<SpriteRenderer>().enabled = false;
            birdControls[index].SetDead(true);
            float middle = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 0)).y;
            if (GetTopPipe() != null)
            {
                middle = (GetTopPipe().transform.position.y + GetBottomPipe().transform.position.y) / 2;
            }
            GA.addFitness(index, fitness);
            if (IsOffScreen(birds[index].transform.position))
            {
                GA.setFitness(index, 0.1f);
            }
        }
    }

    int agentsDead = 0;
    public void Reset()
    {
        Debug.Log(score);
        // reset the score
        score = 0;
        scoreText.text = "" + score;

        // reset the bird
        if (!useGA)
        {
            agentsDead++;
            if (agentsDead >= numberOfAgents)
            {
                agentsDead = 0;
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].transform.position = new Vector3(birdStartingPosition.x, birdStartingPosition.y + Random.Range(-1.0f, 1.0f) * randomPositionAmount);
                    birds[i].GetComponent<BirdControl>().Reset();
                }
                //Debug.Log("Writing fitness to file (" + fitness + ")");
                //file.WriteLine("" + fitness);
                //file.Flush();
                fitness = 0;

                // remove all pipes
                foreach (Transform child in pipes.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                pipeSpawnTime = 0;
                firstNext = true;
            }
            /*
            bird.transform.position = new Vector3(birdStartingPosition.x, birdStartingPosition.y);
            bird.GetComponent<BirdControl>().Reset();
            file.WriteLine("" + fitness);
            file.Flush();
            fitness = 0;
            */
        }
        else
        {
            if (loadFromFile)
            {
                bird.transform.position = new Vector3(birdStartingPosition.x, birdStartingPosition.y);
                bird.GetComponent<SpriteRenderer>().enabled = true;
                birdControl.SetDead(false);
                birdControl.Reset();
            }
            else
            {
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].transform.position = new Vector3(birdStartingPosition.x, birdStartingPosition.y);
                    birds[i].GetComponent<SpriteRenderer>().enabled = true;
                    birdControls[i].SetDead(false);
                    birdControls[i].Reset();
                    fitness = 0;
                }
            }

            // remove all pipes
            foreach (Transform child in pipes.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            pipeSpawnTime = 0;
            firstNext = true;
        }


    }

    public GameObject GetBottomPipe()
    {
        return nextBottom;
    }

    public GameObject GetTopPipe()
    {
        return nextTop;
    }

    public GameObject GetBird()
    {
        return bird;
    }

    public GameObject GetBird(int index)
    {
        return birds[index];
    }
}
