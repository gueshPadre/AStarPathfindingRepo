using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

public class Agent : MonoBehaviour
{
    public static Agent Instance;
    public Queue<Vector3> movingPosQueue;
    public static int goalIndex = 0;

    [Tooltip("empty Gameobject to create the start position when the agent is over with his run")]
    [SerializeField] GameObject emptyGO;
    [Tooltip("Bottom clock timer Canvas")]
    [SerializeField] GameObject textCanvas;         // bottom clockTimer Canvas
    [Tooltip("Green top timer Canvas")]
    [SerializeField] GameObject totalTextCanvas;    //green top timer Canvas
    [Tooltip("List of text Canvas at the end of the run")]
    [SerializeField] GameObject finalTextCanvas;    // List of text Canvas at the end

    Queue<float> finalTimerQueue = new Queue<float>();  //Queue that holds the timers of all the runs
    Queue<float> finalNodeQueue = new Queue<float>();   ////Queue that holds the distance (in nodes) of all the runs

    PathFinding pathScript;
    bool shouldMove = false;
    [SerializeField] float agentSpeed = 3f;
    Vector3 startPos;
    bool isBackHome = false;    // switch to know when we're back to start position

    bool startTimer = false;    // allows to start and stop the timeclock
    float totalTime;        // total time it took for the run
    float timer;        // timeclock
    int nodeCounter = 0;
    float rayLength     //ray length when checking in front. Set to slightly bigger than the diameter
    {
        get
        {
            return (FindObjectOfType<WaypointMaker>().NodeDiameter / 2) + 0.1f;
        }
        set
        {
            rayLength = value;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        startPos = transform.position;
        movingPosQueue = new Queue<Vector3>();
        pathScript = FindObjectOfType<PathFinding>();
        pathScript.MoveToGoal += Agent_MoveToGoal;

    }

    private void Agent_MoveToGoal()
    {
        shouldMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        // moving towards the goal
        HandleAgentMovement();
    }

    private void HandleAgentMovement()
    {
        if (shouldMove && movingPosQueue.Count > 0)
        {
            MoveAgent();
        }
        else    //Done moving
        {
            if (pathScript.IsDone && shouldMove)
            {
                shouldMove = false;
                if ((goalIndex + 1) < pathScript.Goals.Count)       //More destinations to go
                {
                    ManageTimers();
                    goalIndex++;
                    pathScript.IsDone = false;
                }
                else if ((goalIndex + 1) == pathScript.Goals.Count && !isBackHome)  // return home
                {
                    ReturnToStartPos();
                    ManageTimers();
                }
                else
                {
                    // display the last 'leaderboards'
                    DisplayBoard();
                }
            }
        }
    }

    /// <summary>
    /// Moves the Agent based on the moving Queue
    /// </summary>
    void MoveAgent()
    {
        startTimer = true;
        MeasurePerformance();
        Vector3 nextPos = movingPosQueue.Peek();     //use the information without dequeuing it from the list
        if (transform.position != nextPos)
        {
            Scout((nextPos - transform.position).normalized * rayLength);
            transform.position = Vector3.MoveTowards(transform.position, nextPos, Time.deltaTime * agentSpeed);
        }
        else
        {
            movingPosQueue.Dequeue();
            nodeCounter++;
        }
    }

    /// <summary>
    /// Reset timers and saves the timer and number of Nodes travelled
    /// </summary>
    void ManageTimers()
    {
        totalTime = timer;
        timer = 0f;     // reset timer
        ShowTimer();
        totalTextCanvas.GetComponentInChildren<Text>().text = "Timer: " + totalTime.ToString() + "      Nodes Travelled: " + nodeCounter.ToString();
        totalTextCanvas.GetComponentInChildren<Text>().color = Color.green;
        finalTimerQueue.Enqueue(totalTime);
        finalNodeQueue.Enqueue(nodeCounter);
        nodeCounter = 0;
        
    }

    /// <summary>
    /// Compile all the timer information and the nodes information and writes it on a table
    /// </summary>
    void DisplayBoard()
    {
        StringBuilder sb = new StringBuilder();
        string finalText = "";
        float min = finalTimerQueue.Min();
        float max = finalTimerQueue.Max();
        for (int i = finalTimerQueue.Count; i > 0; i--)
        {
            float totalTime = 0f;
            finalTextCanvas.SetActive(true);
            if (finalTimerQueue.Peek() == min)
            {
                finalText = "<color=blue>Run " + i.ToString() + " Timer: " + finalTimerQueue.Peek().ToString("##.##") + " and Node Travelled: " + finalNodeQueue.Peek() + "\n</color>";
            }
            else if (finalTimerQueue.Peek() == max)
            {
                finalText = "<color=red>Run " + i.ToString() + " Timer: " + finalTimerQueue.Peek().ToString("##.##") + " and Node Travelled: " + finalNodeQueue.Peek() + "\n</color>";
            }
            else
            {
                finalText = "Run " + i.ToString() + " Timer: " + finalTimerQueue.Peek().ToString("##.##") + " and Node Travelled: " + finalNodeQueue.Peek() + "\n";
            }
            sb.Append(finalText);
            totalTime += finalTimerQueue.Peek();
            finalTextCanvas.GetComponentInChildren<Text>().text = sb.ToString();
            finalTimerQueue.Dequeue();
            finalNodeQueue.Dequeue();
        }
    }

    /// <summary>
    /// After the run, agent goes back to starting position
    /// </summary>
    void ReturnToStartPos()
    {
        GameObject posGO = Instantiate(emptyGO, startPos, Quaternion.identity);
        pathScript.Goals.Add(posGO.transform);
        goalIndex++;
        pathScript.IsDone = false;
        isBackHome = true;
        startTimer = false;
    }

    void ShowTimer()
    {
        StartCoroutine(DisplayTimer());
    }

    IEnumerator DisplayTimer()
    {
        totalTextCanvas.SetActive(true);
        yield return new WaitForSeconds(1f);
        totalTextCanvas.SetActive(false);
    }

    /// <summary>
    /// Starts timer and puts clock on screen
    /// </summary>
    void MeasurePerformance()
    {
        if (startTimer)
        {
            timer += Time.deltaTime;
            textCanvas.GetComponentInChildren<Text>().text = "Timer: " + timer.ToString("##.##") + "\nNodes Travelled: " + nodeCounter.ToString();
        }
    }

    /// <summary>
    /// Sends a ray in front of the Agent to look for unexpected object
    /// </summary>
    /// <param name="nextDir"> the direction in which the ray is pointing</param>
    void Scout(Vector3 nextDir)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, nextDir);
        if(Physics.Raycast(ray, out hit, rayLength, LayerMask.GetMask("Wall")))
        {
            shouldMove = false;
            pathScript.IsDone = true;
            StartCoroutine(ReCalculateRoute(nextDir));
        }
        //Debug.DrawRay(transform.position, nextDir, Color.green);      // display the ray in the Editor
    }

    // Recalculates the fastest route by taking into account the new obstacle
    IEnumerator ReCalculateRoute(Vector3 nextDir)
    {
        WaypointMaker graph = FindObjectOfType<WaypointMaker>();
        graph.EmptyCurrentGraph();      //Empty the current graph so we can recalculate fully
        transform.position -= nextDir * graph.NodeDiameter * 2;   //backup a bit when hitting an unexpected obstacle
        yield return new WaitForEndOfFrame();
        movingPosQueue.Clear();
        graph.CreateWaypointGraph();
        yield return new WaitForEndOfFrame();   //let time to recalculate before moving again
        pathScript.IsDone = false;
    }
    private void OnDestroy()
    {
        pathScript.MoveToGoal -= Agent_MoveToGoal;
    }
}
