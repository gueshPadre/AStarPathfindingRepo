using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] Transform goalTransform = null;
    float multiplier = 3f;
    Vector3 downwardFrontVector;
    Vector3 downwardFDRVector;  // front diagonal right
    Vector3 downwardSideVector;
    Vector3 downwardFDLVector;  // front diagonal left
    Vector3 downwardLeftSideVector;
    Vector3 downwardBDLVector;  // back diagonal right
    Vector3 downwardBackVector;
    Vector3 downwardBDRVector;  // back diagonal left

    Vector3 frontHitPoint;


    RaycastHit frontHit;
    RaycastHit FDRHit;
    RaycastHit rightSideHit;
    RaycastHit FDLHit;
    RaycastHit leftSideHit;
    RaycastHit BDLHit;
    RaycastHit BackHit;
    RaycastHit BDRHit;

    bool wallInFront;
    bool wallFrontRight;
    bool wallRight;
    bool wallFrontLeft;
    bool wallLeft;
    bool wallBackLeft;
    bool wallBack;
    bool wallBackRight;

    static bool touchFront;
    static bool touchFrontRight;
    static bool touchRight;
    static bool touchFrontLeft;
    static bool touchLeft;
    static bool touchBackLeft;
    static bool touchBack;
    static bool touchBackRight;

    float[] smallerEur;
    static bool shouldMove;
    static Vector3 aim;
    string[] allObjectlayers = { "Grid", "Wall" };

    static int index;
    bool firstTime = false;

    Dictionary<float, Vector3> heuristicPoint = new Dictionary<float, Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        downwardFrontVector = new Vector3(0, -1, 2);
        downwardFDRVector = new Vector3(2, -1, 2);
        downwardSideVector = new Vector3(2, -1, 0);
        downwardFDLVector = new Vector3(-2, -1, 2);
        downwardLeftSideVector = new Vector3(-2, -1, 0);
        downwardBDLVector = new Vector3(-2, -1, -2);
        downwardBackVector = new Vector3(0, -1, -2);
        downwardBDRVector = new Vector3(2, -1, -2);



        CalculateEuristicRays(transform.position, true, allObjectlayers);
    }


    /// <summary>
    /// Makes all the rays and calculate the heuristic of each of them.
    /// </summary>
    /// <param name="pos">position of the rays start position</param>
    /// <param name="shouldMove">allows for calculating without provoking a move from the player</param>
    /// <param name="layerNames">the layers in which the rays can interact</param>
    /// <returns>returns the second closest heuristic after hitting a wall</returns>
    Vector3 CalculateEuristicRays(Vector3 pos, bool shouldMove, params string[] layerNames)
    {
        float frontEur = 0f;
        float FDREur = 0f;
        float rightSideEur = 0f;
        float FDLEur = 0f;
        float leftSideEur = 0f;
        float BDLEur = 0f;
        float backEur = 0f;
        float BDREur = 0f;



        //Front Ray
        Ray frontRay = new Ray(pos, downwardFrontVector * multiplier);
        touchFront = Physics.Raycast(frontRay, out frontHit, LayerMask.GetMask(layerNames));
        if (touchFront)
        {
            if (frontHit.collider.tag == "Wall")
            {
                wallInFront = true;
                print("Front Hit point value in Calculate Ray: " + frontHit.point);
                frontEur = int.MaxValue;
            }
            else
            {
                wallInFront = false;
                frontEur = CalculateManhattanValue(frontHit.point);
                heuristicPoint.Add(frontEur, frontHit.point);
            }
            //print("Front Man value: " + frontEur);
            Debug.DrawRay(pos, downwardFrontVector, Color.black, 1f);
        }


        //Front right diagonal
        Ray FDRray = new Ray(pos, downwardFDRVector * multiplier);
        touchFrontRight = Physics.Raycast(FDRray, out FDRHit, LayerMask.GetMask(layerNames));
        if (touchFrontRight)
        {
            if (FDRHit.collider.tag == "Wall")
            {
                wallFrontRight = true;
                FDREur = int.MaxValue;
            }
            else
            {
                wallFrontRight = false;
                FDREur = CalculateManhattanValue(FDRHit.point);
                heuristicPoint.Add(FDLEur, FDRHit.point);
            }
            //print("FD Man value: " + FDREur);
            Debug.DrawRay(pos, downwardFDRVector, Color.blue, 1f);
        }

        // right Side ray
        Ray rightSideRay = new Ray(pos, downwardSideVector * multiplier);
        touchRight = Physics.Raycast(rightSideRay, out rightSideHit, LayerMask.GetMask(layerNames));
        if (touchRight)
        {
            if (rightSideHit.collider.tag == "Wall")
            {
                wallRight = true;
                rightSideEur = int.MaxValue;
            }
            else
            {
                wallRight = false;
                rightSideEur = CalculateManhattanValue(rightSideHit.point);
                heuristicPoint.Add(rightSideEur,rightSideHit.point);
            }
            //print("Side Man value: " + rightSideEur);
            Debug.DrawRay(pos, downwardSideVector, Color.red, 1f);
        }

        //Front diagonal left
        Ray FDLRay = new Ray(pos, downwardFDLVector * multiplier);
        touchFrontLeft = Physics.Raycast(FDLRay, out FDLHit, LayerMask.GetMask(layerNames));
        if (touchFrontLeft)
        {
            if (FDLHit.collider.tag == "Wall")
            {
                wallFrontLeft = true;
                FDLEur = int.MaxValue;
            }
            else
            {
                wallFrontLeft = false;
                FDLEur = CalculateManhattanValue(FDLHit.point);
                heuristicPoint.Add(FDLEur, FDLHit.point);
            }
        }

        //Left Side Ray
        Ray leftSideRay = new Ray(pos, downwardLeftSideVector * multiplier);
        touchLeft = Physics.Raycast(leftSideRay, out leftSideHit, LayerMask.GetMask(layerNames));
        if (touchLeft)
        {
            if (leftSideHit.collider.tag == "Wall")
            {
                wallLeft = true;
            }
            leftSideEur = CalculateManhattanValue(leftSideHit.point);
        }

        //Back diagonal left
        Ray BDLRay = new Ray(pos, downwardBDLVector * multiplier);
        touchBackLeft = Physics.Raycast(BDLRay, out BDLHit, LayerMask.GetMask(layerNames));
        if (touchBackLeft)
        {
            if (BDLHit.collider.tag == "Wall")
            {
                wallBackLeft = true;
            }
            BDLEur = CalculateManhattanValue(BDLHit.point);
        }

        //Back Ray
        Ray backRay = new Ray(pos, downwardBackVector * multiplier);
        touchBack = Physics.Raycast(backRay, out BackHit, LayerMask.GetMask(layerNames));
        if (touchBack)
        {
            if (BackHit.collider.tag == "Wall")
            {
                wallBack = true;
            }
            backEur = CalculateManhattanValue(BackHit.point);
        }

        //Back right diagonal
        Ray BDRray = new Ray(pos, downwardBDRVector * multiplier);
        touchBackRight = Physics.Raycast(BDRray, out BDRHit, LayerMask.GetMask(layerNames));
        if (touchBackRight)
        {
            if (BDRHit.collider.tag == "Wall")
            {
                wallBackRight = true;
            }
            BDREur = CalculateManhattanValue(BDRHit.point);
        }

        smallerEur = GetEfficiencyList(frontEur, FDREur, rightSideEur, FDLEur, leftSideEur, BDLEur, backEur, BDREur);

        if (shouldMove)
        {
            MoveTowardsTheSmallestEur(frontEur, FDREur, rightSideEur, FDLEur, leftSideEur, BDLEur, backEur, BDREur, smallerEur[0]);
        }
        if (firstTime) { index = 1; } else { index = 0; }
        if (smallerEur[index] == frontEur) { print("frontEur is closer and index is: " + index); return new Vector3(frontHit.point.x, transform.position.y, frontHit.point.z); }
        else if (smallerEur[index] == FDREur) { print("fDR is closer"); return new Vector3(FDRHit.point.x, transform.position.y, FDRHit.point.z); }
        else if (smallerEur[index] == rightSideEur) { print("right is closer"); return new Vector3(rightSideHit.point.x, transform.position.y, rightSideHit.point.z); }
        else if (smallerEur[index] == FDLEur) { print("fDL is closer"); return new Vector3(FDLHit.point.x, transform.position.y, FDLHit.point.z); }
        else if (smallerEur[index] == leftSideEur) { print("left is closer"); return new Vector3(leftSideHit.point.x, transform.position.y, leftSideHit.point.z); }
        else if (smallerEur[index] == BDLEur) { return new Vector3(BDLHit.point.x, transform.position.y, BDLHit.point.z); }
        else if (smallerEur[index] == backEur) { return new Vector3(BackHit.point.x, transform.position.y, BackHit.point.z); }
        else return BDRHit.point;


    }

    float CalculateManhattanValue(Vector3 point)
    {
        //difference in y + difference in x
        return (Mathf.Abs(goalTransform.position.z - point.z)) + (Mathf.Abs(goalTransform.position.x - point.x));
    }


    void MoveTowardsTheSmallestEur(float frontEur, float FDREur, float rightSideEur, float FDLEur, float leftSideEur, float BDLEur,
        float backEur, float BDREur, float smallerEur)
    {
        if (IsSmallestEur(frontEur, smallerEur))
        {
            if (wallInFront)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("GO Front");
            shouldMove = true;
            Vector3 tmp = new Vector3(frontHit.point.x, transform.position.y, frontHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(FDREur, smallerEur))
        {
            if (wallFrontRight)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("GO FDR");
            shouldMove = true;
            Vector3 tmp = new Vector3(FDRHit.point.x, transform.position.y, FDRHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(rightSideEur, smallerEur))
        {
            if (wallRight)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("GO SIDE");
            shouldMove = true;
            Vector3 tmp = new Vector3(rightSideHit.point.x, transform.position.y, rightSideHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(FDLEur, smallerEur))
        {
            if (wallFrontLeft)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("Go FDL");
            shouldMove = true;
            Vector3 tmp = new Vector3(FDLHit.point.x, transform.position.y, FDLHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(leftSideEur, smallerEur))
        {
            if (wallLeft)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("Go Left");
            shouldMove = true;
            Vector3 tmp = new Vector3(leftSideHit.point.x, transform.position.y, leftSideHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(BDLEur, smallerEur))
        {
            if (wallBackLeft)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("Go BDL");
            shouldMove = true;
            Vector3 tmp = new Vector3(BDLHit.point.x, transform.position.y, BDLHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(backEur, smallerEur))
        {
            if (wallBack)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("Go back");
            shouldMove = true;
            Vector3 tmp = new Vector3(BackHit.point.x, transform.position.y, BackHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(BDREur, smallerEur))
        {
            if (wallBackRight)
            {
                shouldMove = false;
                firstTime = true;
                AssessEuristics(transform.position);
                return;
            }
            print("Go BDR");
            shouldMove = true;
            Vector3 tmp = new Vector3(BDRHit.point.x, transform.position.y, BDRHit.point.z);
            aim = tmp;
        }
        else
        {
            print("Confused? " + "frontEur: " + frontEur + ", FDEur: " + FDREur + ", sideEur: " + rightSideEur);
        }
    }


    bool IsSmallestEur(float euristic, float smallest)
    {
        return euristic <= smallest;
    }

    float[] GetEfficiencyList(params float[] euristics)
    {
        Dictionary<float, Vector3> dict = new Dictionary<float, Vector3>();
        foreach (KeyValuePair<float,Vector3> point in heuristicPoint)
        {

        }
        Array.Sort(euristics);
        return euristics;
        //try
        //{
        //    float front = euristics[0];
        //    float FDR = euristics[1];
        //    float rightSide = euristics[2];
        //    float FDL = euristics[3];
        //    float leftSide = euristics[4];

        //for (int i = 0; i < euristics.Length; i++)
        //{
        //    if(front > euristics[i]) { }
        //}
        //if (frontEur <= FDREur && frontEur <= rightSideEur) { return frontEur; }
        //if (FDREur <= frontEur && FDREur <= rightSideEur) { return FDREur; }
        //if (rightSideEur <= frontEur && rightSideEur <= FDREur) { return rightSideEur; }
        //return 0;
        //}
        //catch (IndexOutOfRangeException)
        //{
        //    Debug.Log("One euristic is not being checked");
        //    throw;
        //}
    }

    /// <summary>
    /// Recursive Method checking the best heuristics around when an object was hit
    /// until obstacle is not around anymore
    /// </summary>
    /// <param name="playerPos"></param>
    void AssessEuristics(Vector3 playerPos)
    {
        if (firstTime)
        {
            shouldMove = false;
        }
        else
        {
            shouldMove = true;
            Vector3 tmp = new Vector3(playerPos.x, transform.position.y, playerPos.z);
            aim = tmp;
        }
        // Base case
        if ((!touchFront || !touchFrontRight || !touchRight || !touchFrontLeft || !touchLeft || !touchBackLeft || !touchBack || !touchBackRight)
            || (!wallInFront && !wallFrontRight && !wallFrontLeft && !wallRight && !wallLeft && !wallBackLeft && !wallBack && !wallBackRight))
        {
            shouldMove = true;
            Vector3 tmp = new Vector3(playerPos.x, transform.position.y, playerPos.z);
            aim = tmp;
            print("Hit the base case with aim of: " + aim);
            return;
        }
        Vector3 newPosition = CalculateEuristicRays(playerPos, false, allObjectlayers);
        Debug.LogError("Recusrsive position is: " + newPosition);
        firstTime = false;
        AssessEuristics(newPosition);
        //if (wallInFront)
        //{
        //}
        //else if (wallFrontRight) { AssessEuristics(FDRHit.point); }
        //else if (wallFrontLeft) { AssessEuristics(FDLHit.point); }
        //else if (wallRight) { AssessEuristics(rightSideHit.point); }
        //else if (wallLeft) { AssessEuristics(leftSideHit.point); }
        //else if (wallBackLeft) { AssessEuristics(BDLHit.point); }
        //else if (wallBack) { AssessEuristics(BackHit.point); }
        //else if (wallBackRight) { AssessEuristics(BDRHit.point); }
        //else
        //{
        //    print("No wall is being touched");
        //    return;
        //}

    }


    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {
            if (transform.position != aim)
            {
                transform.position = Vector3.MoveTowards(transform.position, aim, Time.deltaTime);
            }
            else
            {
                shouldMove = false;
                CalculateEuristicRays(transform.position, true, allObjectlayers);
            }
        }
    }
}
