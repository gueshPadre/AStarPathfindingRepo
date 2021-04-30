using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public static Agent Instance;
    public Queue<Vector3> movingPos;
    public static int goalIndex = 0;
    public static bool IsArrived = true;
    [Tooltip("empty Gameobject to create the start position when the agent is over with his run")]
    [SerializeField] GameObject emptyGO;
    [SerializeField] float rotationSpeed;

    PathFinding pathScript;
    bool shouldMove = false;
    [SerializeField] float agentSpeed = 3f;
    Vector3 startPos;
    bool isBackHome = false;
    bool startTimer = false;
    float timeAtStart;
    float totalTime;

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
        movingPos = new Queue<Vector3>();
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
        if (shouldMove && movingPos.Count > 0)
        {
            MeasurePerformance();
            Vector3 nextPos = movingPos.Peek();
            if (transform.position != nextPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, Time.deltaTime * agentSpeed);
                //Quaternion toRotation = Quaternion.LookRotation(nextPos);
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime * rotationSpeed);
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position), 1);
            }
            else
            {
                movingPos.Dequeue();
            }
        }
        else    //Done moving
        {
            if (pathScript.IsDone && shouldMove)
            {
                shouldMove = false;
                if ((goalIndex + 1) < pathScript.Goals.Count)       //More destinations to go
                {
                    totalTime = timeAtStart - Time.time;
                    print("Done and took: " + totalTime);
                    goalIndex++;
                    pathScript.IsDone = false;
                }
                else if ((goalIndex + 1) == pathScript.Goals.Count && !isBackHome)  // return home
                {
                    ReturnToStartPos();
                }
            }
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
    }


    void MeasurePerformance()
    {
        if (!startTimer)
        {
            timeAtStart = Time.time;
            startTimer = true;
        }
    }


    private void OnDestroy()
    {
        pathScript.MoveToGoal -= Agent_MoveToGoal;
    }
}
