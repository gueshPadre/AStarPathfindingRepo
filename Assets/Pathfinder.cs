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

    static float[] smallerEur;
    static bool shouldMove;
    static Vector3 aim;
    string[] allObjectlayers = { "Grid", "Wall" };
    static List<float> heurList = new List<float>();

    static int index = 0;
    Vector3 pastPos;
    static float hCost = 0f;
    static float fCost = 0f;

    Dictionary<Vector3, float> heuristicToPointDict = new Dictionary<Vector3, float>();
    Dictionary<Vector3, bool> heuristicAvailableDict = new Dictionary<Vector3, bool>();

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



        CalculateEuristicRays(transform.position);
    }

    /// <summary>
    /// Makes all the rays and calculate the heuristic of each of them.
    /// </summary>
    /// <param name="pos">position of the rays start position</param>
    /// <param name=""="iterationNumber">decides which heuristic to chose. 0 = the smallest one</param>
    /// <returns>returns the second closest heuristic after hitting a wall</returns>
    void CalculateEuristicRays(Vector3 pos)
    {
        SendRays(pos);
        //if (wallInFront || wallFrontLeft || wallFrontRight || wallRight || wallLeft || wallBack || wallBackLeft || wallBackRight)
        //{
        //    index++;
        //    foreach (KeyValuePair<Vector3, float> point in heuristicToPointDict)
        //    {
        //        print("the key: " + point.Key + " = " + point.Value);
        //        if (smallerEur[index] == point.Value) { nextPos = point.Key; print("FOUND IT!"); break; }
        //    }
        //    foreach (KeyValuePair<Vector3, bool> point in heuristicAvailableDict)
        //    {
        //        print("is this Vector3: " + point.Key + " available? " + point.Value);
        //    }
        //    if (heuristicAvailableDict.ContainsKey(nextPos))
        //    {
        //        print("Contains key?: " + nextPos);
        //        nextPos = new Vector3(nextPos.x, transform.position.y, nextPos.z);
        //        CalculateEuristicRays(nextPos, index);
        //    }
        //    //for (int i = 0; i < smallerEur.Length; i++)
        //    //{
        //    //    print("listcount: " + smallerEur[i] + " nb of times: " + i);
        //    //}
        //    Debug.LogError("heuristic dict count: " + heuristicToPointDict.Count + " and the number: " + index + " of the heurList is : " + smallerEur[index] + " with a count of" +
        //      " : " + smallerEur.Length + " and nextPos is: " + nextPos);
        //    return;
        //}
        //print("smallest heuristic is: " + smallerEur[index]);
        //MoveTowardsTheSmallestEur(frontEur, FDREur, rightSideEur, FDLEur, leftSideEur, BDLEur, backEur, BDREur, smallerEur[index]);
        MoveAgent();
    }

    float SendRays(Vector3 pos)
    {
        print("SendRays " + pos);
        float frontEur = 0f;
        float FDREur = 0f;
        float rightSideEur = 0f;
        float FDLEur = 0f;
        float leftSideEur = 0f;
        float BDLEur = 0f;
        float backEur = 0f;
        float BDREur = 0f;
        hCost = 0;      //10 for straight and 14 for diagonals
        heuristicToPointDict = new Dictionary<Vector3, float>();
        heuristicAvailableDict = new Dictionary<Vector3, bool>();

        //Front Ray
        Ray frontRay = new Ray(pos, downwardFrontVector * multiplier);
        touchFront = Physics.Raycast(frontRay, out frontHit, LayerMask.GetMask(allObjectlayers));
        if (touchFront)
        {
            wallInFront = false;
            if (frontHit.collider.tag == "Wall")
            {
                wallInFront = true;
                heuristicAvailableDict.Add(frontHit.point, false);
                frontEur = int.MaxValue;
                print("Touching front wall " + frontHit.point);
            }
            else
            {
                hCost += 10;
                frontEur = CalculateManhattanValue(frontHit.point);
            }
            heuristicToPointDict.Add(frontHit.point, frontEur);
            Debug.DrawRay(pos, downwardFrontVector, Color.black, 1f);
        }


        //Front right diagonal
        Ray FDRray = new Ray(pos, downwardFDRVector * multiplier);
        touchFrontRight = Physics.Raycast(FDRray, out FDRHit, LayerMask.GetMask(allObjectlayers));
        if (touchFrontRight)
        {
            if (FDRHit.collider.tag == "Wall")
            {
                wallFrontRight = true;
                heuristicAvailableDict.Add(FDRHit.point, false);
                FDREur = int.MaxValue;
            }
            else
            {
                hCost += 14;
                FDREur = CalculateManhattanValue(FDRHit.point);
                wallFrontRight = false;
                //if (!heuristicToPointDict.ContainsKey(FDRHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(FDRHit.point, FDREur);
            //print("FD Man value: " + FDREur);
            Debug.DrawRay(pos, downwardFDRVector, Color.blue, 1f);
        }

        // right Side ray
        Ray rightSideRay = new Ray(pos, downwardSideVector * multiplier);
        touchRight = Physics.Raycast(rightSideRay, out rightSideHit, LayerMask.GetMask(allObjectlayers));
        if (touchRight)
        {
            if (rightSideHit.collider.tag == "Wall")
            {
                wallRight = true;
                heuristicAvailableDict.Add(rightSideHit.point, false);
                rightSideEur = int.MaxValue;
            }
            else
            {
                hCost += 10;
                rightSideEur = CalculateManhattanValue(rightSideHit.point);
                wallRight = false;
                //if (!heuristicToPointDict.ContainsKey(rightSideHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(rightSideHit.point, rightSideEur);
            Debug.DrawRay(pos, downwardSideVector, Color.red, 1f);
        }

        //Front diagonal left
        Ray FDLRay = new Ray(pos, downwardFDLVector * multiplier);
        touchFrontLeft = Physics.Raycast(FDLRay, out FDLHit, LayerMask.GetMask(allObjectlayers));
        if (touchFrontLeft)
        {
            if (FDLHit.collider.tag == "Wall")
            {
                wallFrontLeft = true;
                heuristicAvailableDict.Add(FDLHit.point, false);
                FDLEur = int.MaxValue;
            }
            else
            {
                hCost += 14;
                FDLEur = CalculateManhattanValue(FDLHit.point);
                wallFrontLeft = false;
                //if (!heuristicToPointDict.ContainsKey(FDLHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(FDLHit.point, FDLEur);
        }

        //Left Side Ray
        Ray leftSideRay = new Ray(pos, downwardLeftSideVector * multiplier);
        touchLeft = Physics.Raycast(leftSideRay, out leftSideHit, LayerMask.GetMask(allObjectlayers));
        if (touchLeft)
        {
            if (leftSideHit.collider.tag == "Wall")
            {
                wallLeft = true;
                heuristicAvailableDict.Add(leftSideHit.point, false);
                leftSideEur = int.MaxValue;
            }
            else
            {
                hCost += 10;
                leftSideEur = CalculateManhattanValue(leftSideHit.point);
                wallLeft = false;
                //if (!heuristicToPointDict.ContainsKey(leftSideHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(leftSideHit.point, leftSideEur);
        }

        //Back diagonal left
        Ray BDLRay = new Ray(pos, downwardBDLVector * multiplier);
        touchBackLeft = Physics.Raycast(BDLRay, out BDLHit, LayerMask.GetMask(allObjectlayers));
        if (touchBackLeft)
        {
            if (BDLHit.collider.tag == "Wall")
            {
                wallBackLeft = true;
                heuristicAvailableDict.Add(BDLHit.point, false);
                BDLEur = int.MaxValue;
            }
            else
            {
                hCost += 14;
                BDLEur = CalculateManhattanValue(BDLHit.point);
                wallBackLeft = false;
                //if (!heuristicToPointDict.ContainsKey(BDLHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(BDLHit.point, BDLEur);
        }

        //Back Ray
        Ray backRay = new Ray(pos, downwardBackVector * multiplier);
        touchBack = Physics.Raycast(backRay, out BackHit, LayerMask.GetMask(allObjectlayers));
        if (touchBack)
        {
            if (BackHit.collider.tag == "Wall")
            {
                wallBack = true;
                heuristicAvailableDict.Add(BackHit.point, false);
                backEur = int.MaxValue;
            }
            else
            {
                hCost += 10;
                backEur = CalculateManhattanValue(BackHit.point);
                wallBack = false;
                //if (!heuristicToPointDict.ContainsKey(BackHit.point))
                //{
                //}
            }
            heuristicToPointDict.Add(BackHit.point, backEur);
        }

        //Back right diagonal
        Ray BDRray = new Ray(pos, downwardBDRVector * multiplier);
        touchBackRight = Physics.Raycast(BDRray, out BDRHit, LayerMask.GetMask(allObjectlayers));
        if (touchBackRight)
        {
            if (BDRHit.collider.tag == "Wall")
            {
                wallBackRight = true;
                heuristicAvailableDict.Add(BDRHit.point, false);
                BDREur = int.MaxValue;
            }
            else
            {
                hCost += 14;
                BDREur = CalculateManhattanValue(BDRHit.point);
                wallBackRight = false;
            }
            heuristicToPointDict.Add(BDRHit.point, BDREur);
        }

        smallerEur = GetEfficiencyList(frontEur, FDREur, rightSideEur, FDLEur, leftSideEur, BDLEur, backEur, BDREur);
        while (smallerEur[index] == int.MaxValue)
        {
            index++;
            if (index > smallerEur.Length) { Debug.LogError("surrounded by walls? "); break; }
        }
        Dictionary<float, Vector3> fCostDict = new Dictionary<float, Vector3>();
        List<float> fCostList = new List<float>();
        for (int i = 0; i < smallerEur.Length; i++)
        {
            Vector3 tmpPos = Vector3.zero;
            fCost = smallerEur[i] + CalculateHCost(smallerEur[i]);
            print("fCost: " + fCost);
            foreach (KeyValuePair<Vector3, float> point in heuristicToPointDict)
            {
                if (point.Value == smallerEur[i])
                {
                    tmpPos = point.Key;
                }
            }
            fCostList.Add(fCost);
            if (!fCostDict.ContainsKey(fCost))
            {
                fCostDict.Add(fCost, tmpPos);
            }
        }
        float best = heuristicToPointDict[fCostDict[GetEfficiencyList(fCost)[0]]];
        if (!wallInFront || !wallFrontLeft || !wallFrontRight || !wallRight || !wallLeft || !wallBack || !wallBackLeft || !wallBackRight) { print("not touching any walls"); return best; }
        else
        {
            print("im touching a wall therefore recalcultate");
            return SendRays(fCostDict[GetEfficiencyList(fCost)[0]]);
        }

    }

    float CalculateHCost(float heurPoint)
    {
        Vector3 pos = Vector3.zero;
        foreach (KeyValuePair<Vector3, float> point in heuristicToPointDict)
        {
            if (point.Value == heurPoint)
            {
                pos = point.Key;
            }
        }
        return (transform.position.x - pos.x) + (transform.position.z - pos.z);
    }

    float extender;

    void MoveAgent()
    {
        Vector3 nextPos;
        float opt;
        foreach (KeyValuePair<Vector3, float> point in heuristicToPointDict)
        {
            //print("the key: " + point.Key + " = " + point.Value + " and the smallest heur is: " + smallerEur[index] + " index nb: " + index);
            if (smallerEur[index] == point.Value)
            {
                nextPos = point.Key; print("FOUND IT! " + nextPos);
                if (wallInFront)
                {
                    print("is wallInFront? " + wallInFront);
                    extender += 0.2f;
                    wallInFront = false;
                    opt = SendRays(new Vector3(nextPos.x + extender, transform.position.y, nextPos.z));
                    shouldMove = true;
                    foreach (KeyValuePair<Vector3,float> heur in heuristicToPointDict)
                    {
                        if(opt == heur.Value)
                        {
                            nextPos = heur.Key;
                        }
                        Vector3 tmp = new Vector3(nextPos.x, transform.position.y, nextPos.z);
                        aim = tmp;
                    }
                }
                else
                {
                    extender = 0;
                    index = 0;
                    shouldMove = true;
                    Vector3 tmp = new Vector3(nextPos.x, transform.position.y, nextPos.z);
                    aim = tmp;
                }

            }
        }

    }

    Vector3 newPos;

    void ExploreSide(Vector3 pos)
    {
        float smallestHeur = SendRays(pos);
        Vector3 tmp = Vector3.zero;
        int i = 0;
        do
        {
            foreach (KeyValuePair<Vector3, float> point in heuristicToPointDict)
            {
                if (smallestHeur == point.Value)
                {
                    print("points explored: " + point.Key);
                    tmp = new Vector3(point.Key.x, transform.position.y, point.Key.z);
                }
            }
            i++;
            SendRays(tmp);
            if (i >= 100) { print("wallFront? " + wallInFront); break; }
        } while (wallInFront);
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
            //if (wallInFront)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("GO Front");
            shouldMove = true;
            Vector3 tmp = new Vector3(frontHit.point.x, transform.position.y, frontHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(FDREur, smallerEur))
        {
            //if (wallFrontRight)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("GO FDR");
            shouldMove = true;
            Vector3 tmp = new Vector3(FDRHit.point.x, transform.position.y, FDRHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(rightSideEur, smallerEur))
        {
            //if (wallRight)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("GO SIDE");
            shouldMove = true;
            Vector3 tmp = new Vector3(rightSideHit.point.x, transform.position.y, rightSideHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(FDLEur, smallerEur))
        {
            //if (wallFrontLeft)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("Go FDL");
            shouldMove = true;
            Vector3 tmp = new Vector3(FDLHit.point.x, transform.position.y, FDLHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(leftSideEur, smallerEur))
        {
            //if (wallLeft)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("Go Left");
            shouldMove = true;
            Vector3 tmp = new Vector3(leftSideHit.point.x, transform.position.y, leftSideHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(BDLEur, smallerEur))
        {
            //if (wallBackLeft)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("Go BDL");
            shouldMove = true;
            Vector3 tmp = new Vector3(BDLHit.point.x, transform.position.y, BDLHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(backEur, smallerEur))
        {
            //if (wallBack)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
            print("Go back");
            shouldMove = true;
            Vector3 tmp = new Vector3(BackHit.point.x, transform.position.y, BackHit.point.z);
            aim = tmp;
        }
        else if (IsSmallestEur(BDREur, smallerEur))
        {
            //if (wallBackRight)
            //{
            //    shouldMove = false;
            //    firstTime = true;
            //    AssessEuristics(transform.position);
            //    return;
            //}
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
        Array.Sort(euristics);
        return euristics;
    }

    /// <summary>
    /// Recursive Method checking the best heuristics around when an object was hit
    /// until obstacle is not around anymore
    /// </summary>
    /// <param name="playerPos"></param>
    //void AssessEuristics(Vector3 playerPos)
    //{
    //    if (firstTime)
    //    {
    //        shouldMove = false;
    //    }
    //    else
    //    {
    //        shouldMove = true;
    //        Vector3 tmp = new Vector3(playerPos.x, transform.position.y, playerPos.z);
    //        aim = tmp;
    //    }
    //    // Base case
    //    if ((!touchFront || !touchFrontRight || !touchRight || !touchFrontLeft || !touchLeft || !touchBackLeft || !touchBack || !touchBackRight)
    //        || (!wallInFront && !wallFrontRight && !wallFrontLeft && !wallRight && !wallLeft && !wallBackLeft && !wallBack && !wallBackRight))
    //    {
    //        shouldMove = true;
    //        Vector3 tmp = new Vector3(playerPos.x, transform.position.y, playerPos.z);
    //        aim = tmp;
    //        print("Hit the base case with aim of: " + aim);
    //        return;
    //    }
    //Vector3 newPosition = CalculateEuristicRays(playerPos, 0, allObjectlayers);
    //Debug.LogError("Recusrsive position is: " + newPosition);
    //firstTime = false;
    //AssessEuristics(newPosition);
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

    //}


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
                if (!wallInFront)
                {
                    CalculateEuristicRays(transform.position);
                }
            }
        }
    }
}
