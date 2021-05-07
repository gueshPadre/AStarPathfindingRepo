using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWalls : MonoBehaviour
{
    [SerializeField] Transform waypointOne, waypointTwo = null;
    [SerializeField] float moveSpeed = 3f;

    enum PhaseSelect
    {
        One,
        Two
    }

    PhaseSelect phase; 
    // Start is called before the first frame update
    void Start()
    {
        GetPositions();
        phase = PhaseSelect.One;
    }

    Vector3 firstPos, secondPos;
    void GetPositions()
    {
        firstPos = waypointOne.position;
        secondPos = waypointTwo.position;
    }

    // Update is called once per frame
    void Update()
    {
        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (phase == PhaseSelect.One && transform.position != firstPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, firstPos, Time.deltaTime * moveSpeed);
        }
        else if (phase == PhaseSelect.One && transform.position == firstPos)
        {
            phase = PhaseSelect.Two;
        }

        if (phase == PhaseSelect.Two && transform.position != secondPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, secondPos, Time.deltaTime * moveSpeed);
        }
        else if (phase == PhaseSelect.Two && transform.position == secondPos)
        {
            phase = PhaseSelect.One;
        }
    }
}
