using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SafeCylinderController : NetworkBehaviour
{
    public static float[] radius = {200, 170, 120, 80, 50, 24, 0, 0};
    public static int level = 0;
    public static int MAX_LEVEL = 6;
    public static float[] hurtValue = {1, 2, 3, 4, 5, 6};
    public static float[] center = {0, 1.5f, 0};
    public static float curRadius = 300;
    public static bool isMoving = false;
    public static float FLUSH_SAFE_AREA_TIME = 2;
    public static float safeAreaTimer;
    public static float centerXMoveRate, centerZMoveRate, radiusMoveRate;
    public static int moveTimes, curMoveTimes;
    public static bool calculateSafeAreaEnd = false;
    public static int randomNum = 345;
    void Start()
    {
        safeAreaTimer = FLUSH_SAFE_AREA_TIME;
        if(this.tag == "SafeOut")
        {
            GetNewCenter();
        }
    }

    void Update()
    {
        if(this.tag == "SafeOut")
        {
            SafeOutMove();
        }
        if(this.tag == "SafeIn")
        {
            SetNewSafeIn();
        }
    }

    private void SafeOutMove()
    {
        if(isMoving)
        {
            curMoveTimes++;
            SafeAreaMove();
            if(curMoveTimes == moveTimes){
                level++;
                GetNewCenter();
                safeAreaTimer = FLUSH_SAFE_AREA_TIME;
                isMoving = false;
                // Debug.Log("moving end next time is " + safeAreaTimer);
            }
            return;
        }
        else
        {
            safeAreaTimer -= Time.deltaTime;
            if(safeAreaTimer <= 0.0f && level != MAX_LEVEL)
            {
                isMoving = true;
                // Debug.Log("beginning next move");
            }
        }
    }

    private void SafeAreaMove()
    {
        Vector3 newScale = new Vector3 (transform.localScale.x - radiusMoveRate, 2, transform.localScale.z - radiusMoveRate);
        transform.localScale = newScale;
        // Debug.Log("new scale " + newScale);
        Vector3 newCenter = new Vector3(transform.localPosition.x + centerXMoveRate, 1.5f, transform.localPosition.z + centerZMoveRate);
        transform.localPosition = newCenter;
        // Debug.Log("new center " + newCenter);
    }

    private void GetNewCenter()
    {
        if(level == MAX_LEVEL)
        {
            return;
        }
        int radiusOffset = (int)((radius[level] - radius[level + 1]) / 2.82f);
        // int centerOffsetX = Random.Range(0, 1000) % (radiusOffset * 2) - radiusOffset;
        // int centerOffsetZ = Random.Range(0, 1000) % (radiusOffset * 2) - radiusOffset;
        int centerOffsetX = randomNum % (radiusOffset * 2) - radiusOffset;
        int centerOffsetZ = randomNum % (radiusOffset * 2) - radiusOffset;
        // Debug.Log("radius Level " + radius[level] + " " + radius[level+1]);
        // Debug.Log("radius offset " + radiusOffset);
        // Debug.Log("offset x & z " + (centerOffsetX) + " " + (centerOffsetZ));
        // Debug.Log("old center " + center[0] + " " + center[2]);
        float centerXTemp = center[0];
        float centerZTemp = center[2];
        center[0] = center[0] + centerOffsetX;
        center[2] = center[2] + centerOffsetZ;
        // Debug.Log("new center " + center[0] + " " + center[2]);
        moveTimes = (int)(2 / Time.deltaTime);
        // Debug.Log("time deltaTime " + Time.deltaTime + "move times " + moveTimes);
        curMoveTimes = 0;
        centerXMoveRate = (center[0] - centerXTemp) / moveTimes;
        centerZMoveRate = (center[2] - centerZTemp) / moveTimes;
        radiusMoveRate = (transform.localScale.x - radius[level + 1] + 0.0f) / moveTimes;
        calculateSafeAreaEnd = true;
        // Debug.Log("move rate " + centerXMoveRate + " " + centerZMoveRate + " " + radiusMoveRate);
    }
    private void SetNewSafeIn()
    {
        if(calculateSafeAreaEnd)
        {
            transform.localScale = new Vector3(radius[level+1], 2.0f, radius[level+1]);
            transform.localPosition = new Vector3(center[0], center[1], center[2]);
            calculateSafeAreaEnd = false;
        }
    }
}
