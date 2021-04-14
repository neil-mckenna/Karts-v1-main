using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{

    public Circuit circuit;
    Drive ds;
    public float brakingSensitivity = 1.1f;
    public float steeringSensitivity = 0.01f;
    public float accelSensitivity = 0.03f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;


    // tracker attributes
    GameObject tracker;
    int currentTrackerWP = 0;
    public float lookAhead = 10f;

    // checkpoint and reset variables
    float lastTimeMoving = 0f;
    CheckpointManager cpm;

    float finishSteer;

    // Start is called before the first frame update
    void Start()
    {
        if(circuit == null)
        {
            circuit = GameObject.FindGameObjectWithTag("circuit").GetComponent<Circuit>();
        }

        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);


        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = ds.rb.gameObject.transform.position;
        tracker.transform.rotation = ds.rb.gameObject.transform.rotation;

        this.GetComponent<Ghost>().enabled = false;
        finishSteer = Random.Range(-1.0f, 1.0f);
    }

    void ProgressTracker()
    {
        //Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position);

        if(Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead)
        {
            return; 
        }

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1f); //tracker speed

        if(Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 1f)
        {
            currentTrackerWP++;
            if(currentTrackerWP >= circuit.waypoints.Length)
            {
                currentTrackerWP = 0;
            }
        }

    }

    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!RaceMonitor.racing) 
        {
            lastTimeMoving = Time.time; 
            return;           
        }

        if(cpm == null)
        {
            cpm = ds.rb.GetComponent<CheckpointManager>();
        }

        // Ai stopping at finish end
        if(cpm.lap == RaceMonitor.totalLaps + 1)
        {
            ds.highAccel.Stop();
            ds.Go(0,finishSteer, 0f);
            return;

        }


        ProgressTracker();

        Vector3 localTarget;
        
        float targetAngle;
       

        if(ds.rb.velocity.magnitude > 1)
        {
            lastTimeMoving = Time.time;
        }

        if(Time.time > lastTimeMoving + 4 || ds.rb.gameObject.transform.position.y < -5f || ds.rb.gameObject.transform.position.y > 25f)
        {
            

            ds.rb.transform.position = cpm.lastCp.transform.position + Vector3.up * 2 + new Vector3( Random.Range(-8,8),0,0 );
            ds.rb.transform.rotation = cpm.lastCp.transform.rotation;
            tracker.transform.position = cpm.lastCp.transform.position;
            ds.rb.gameObject.layer = 8;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }

        if(Time.time < ds.rb.GetComponent<AvoidDetector>().avoidTime)
        {
            localTarget = tracker.transform.right * ds.rb.GetComponent<AvoidDetector>().avoidPath;
            
        }
        else
        {
            localTarget = ds.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);
 
        }
        

        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        
        
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);

        float speedFactor = ds.currentSpeed / ds.maxSpeed;

        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0, 90);
        
        //Debug.LogWarning("name " + gameObject.name +" corner angle: " + corner);
        float cornerFactor = corner / 90.0f;

        
        

        float brake = 0f;
        float accel = 1f;

        if(corner > 5 && speedFactor > 0.3f)
        {
            brake = Mathf.Lerp(0, 1 + (speedFactor * speedFactor) * (brakingSensitivity * 5), (2 * cornerFactor * cornerFactor * cornerFactor));
            
        }

        if(corner > 9 && speedFactor > 0.3f)
        {
            brake = Mathf.Lerp(0.4f, 1 + (speedFactor * speedFactor) * (brakingSensitivity * 5), (2 * cornerFactor * cornerFactor * cornerFactor));
            
        }

        if(corner > 12 && speedFactor > 0.3f)
        {
            brake = Mathf.Lerp(0.6f, 1 + (speedFactor * speedFactor) * (brakingSensitivity * brakingSensitivity * 5),  1 - (2 * cornerFactor * cornerFactor * cornerFactor));
            
            
        }

        if(corner > 15 && speedFactor > 0.3f)
        {
            brake = Mathf.Lerp(0.7f, 1 + (speedFactor * speedFactor) * (brakingSensitivity *  brakingSensitivity * 5), 1 - (2 * cornerFactor * cornerFactor));
            
            
        }

        // tight cornering
        if(cornerFactor == 1 && speedFactor > 0.1f)
        {
            brake = 1f;
            accel = 0.1f;
        }

        float prevTorque = ds.torque;
        // uphill helper
        if(speedFactor < 0.3f && ds.rb.gameObject.transform.forward.y > 0.1f)
        {
            ds.torque *= 3.0f;
            accel = 1f;
            brake = 0f;
        }

        // downhill AI
        if(speedFactor < 0.3f && ds.rb.gameObject.transform.forward.y < -0.35f || target.y < -0.35f )
        {
            
            accel = 0.2f;
            brake = 0.9f;
        }

        if(gameObject.name == "GoldJeep")
        {
            Debug.Log(gameObject.name + " corner factor = " + cornerFactor);
            //Debug.LogWarning("brake " + brake + " SF " + speedFactor * brakingSensitivity);


        }

        //Debug.LogWarning(gameObject.name + " brake: " + brake + " accel: " + accel);

        

        

        ds.Go(accel, steer, brake);

        ds.CheckForSkid();
        ds.CalculateEngineSound();
        
        ds.torque = prevTorque;
    }

    private void OnCollisionStay(Collision other) 
    {
        
        if(other.gameObject.name == "barrierWall")
        {
            float accel = 1f;
            float brake = 0f;
            float steer = 0f;

            Debug.LogWarning("collided with barrier");
            ds.Go(-accel, steer, brake);
        }
        if(other.gameObject.tag == "car")
        {
            float accel = 1f;
            float brake = 0f;
            float steer = 0f;

            Debug.LogWarning("Hit other car");
            ds.Go(-accel, steer, brake);
        }
        
    }
  
    

}
