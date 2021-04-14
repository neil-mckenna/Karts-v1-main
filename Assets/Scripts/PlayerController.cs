using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Drive ds;
    float lastTimeMoving = 0f;
    
    Vector3 lastPosition;
    Quaternion lastRotation;
    Rigidbody rb;

    CheckpointManager cpm;
    float finishSteer;

    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponent<Drive> ();
        rb = this.GetComponentInChildren<Rigidbody>();
        this.GetComponent<Ghost>().enabled = false;
        finishSteer = Random.Range(-0.1f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(cpm == null)
        {
            cpm = ds.rb.GetComponent<CheckpointManager>();
        }

        // Player stopping at lap finish
        if(cpm.lap == RaceMonitor.totalLaps + 1)
        {
            ds.highAccel.Stop();
            ds.Go(0,finishSteer, 0f);
            return;

        }
        
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        if(ds.rb.velocity.magnitude > 1 || !RaceMonitor.racing)
        {
            lastTimeMoving = Time.time;
        }
        
        RaycastHit hit;
        Vector3 downwards = transform.TransformDirection(-Vector3.up);

        if(Physics.Raycast(ds.rb.transform.position, downwards, out hit, 10f))
        {
            
            //Debug.DrawRay(rb.transform.position, downwards * 10, Color.yellow);
            //Debug.LogWarning("tag  : " + hit.collider.tag);
            
            string colliderTag = hit.collider.tag;

            if(colliderTag == "road")
            {
                //capture last road positions
                lastPosition = ds.rb.gameObject.transform.position;
                lastRotation = ds.rb.gameObject.transform.rotation;
            }
        }

        // reset car if stuck for 4 seconds 
        if(Time.time > lastTimeMoving + 4 || ds.rb.gameObject.transform.position.y < -5f || ds.rb.gameObject.transform.position.y > 25f)
        {
            
            
            ds.rb.gameObject.transform.position = cpm.lastCp.transform.position + Vector3.up * 2;
            ds.rb.gameObject.transform.rotation = cpm.lastCp.transform.rotation;

            ds.rb.gameObject.layer = 8;
            this.GetComponent<Ghost>().enabled = true;
            Invoke(nameof(ResetLayer), 3f);
        }
        

        // this stop car movement and steering during the countdown
        if(!RaceMonitor.racing) 
        { 
            a = 0;
            s = 0;
            b = 0;    
        }

        ds.Go(a,s,b);

        ds.CheckForSkid();
        ds.CalculateEngineSound();

        
        
    }
}
