using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{

    public int lap = 0;
    public int checkpoint = -1;
    public float timeEntered = 0f;
    int checkPointCount;
    int nextCheckPoint;
    public GameObject lastCp;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] cps = GameObject.FindGameObjectsWithTag("checkpoint");
        checkPointCount = cps.Length;
        foreach (GameObject c in cps)
        {
            if(c.name == "0")
            {
                lastCp = c;
                break;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "checkpoint")
        {
            //Debug.Log("hit checkpoint");
            int thisCPNumber = int.Parse(other.gameObject.name);
            if(thisCPNumber == nextCheckPoint)
            {
                lastCp = other.gameObject;
                checkpoint = thisCPNumber;
                timeEntered = Time.time;
                if(checkpoint == 0) lap++;

                nextCheckPoint++;
                if(nextCheckPoint >= checkPointCount)
                {
                    nextCheckPoint = 0;
                }
            }


        }
        
    }
}
