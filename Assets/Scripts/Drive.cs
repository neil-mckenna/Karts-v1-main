using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour
{
    public WheelCollider[] WheelColliders;
    public GameObject[] WheelMeshs;
    public float torque = 200f;
    public float maxSteerAngle = 30f;
    public float maxBrakeTorque = 500f;

    public AudioSource skidSFX;
    public AudioSource highAccel;

    public Transform SkidTrailPrefab;
    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public GameObject brakeLight1;
    public GameObject brakeLight2;

    public Rigidbody rb;
    public float gearLength = 3f;
    public float currentSpeed { get { return rb.velocity.magnitude * gearLength; } }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numGears = 5;
    float rpm;
    int currentGear = 1;
    float currentGearPerc;
    public float maxSpeed = 200f;
    public float steeringSensitivity = 0.25f;

    public GameObject playerNamePrefab;
    public Renderer jeepMesh;

    public string networkName = "";
    string[] aiNames = { "Amy", "Sheldon", "Howard", "Penny", "Leonard", "Raj", "Bernadette", "Stuart", "Leslie", "Bert", "Barry", "Priya", "Emily", "Wheaton" };



    Transform[] skidTrails = new Transform[4];

    public void StartSkidTrail(int i)
    {
        if(skidTrails[i] == null)
        {
            skidTrails[i] = Instantiate(SkidTrailPrefab);

        }
        skidTrails[i].parent = WheelColliders[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90,0,0);
        skidTrails[i].localPosition = -Vector3.up * WheelColliders[i].radius;

    }

    public void EndSkidTrail(int i)
    {
        if(skidTrails[i] == null)
        {
            return;
        }

        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.rotation = Quaternion.Euler(90,0,0);
        Destroy(holder.gameObject, 30);


    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }

        brakeLight1.SetActive(false);
        brakeLight2.SetActive(false);

        GameObject playerName = Instantiate(playerNamePrefab);
        playerName.GetComponent<NameUIController>().target = rb.gameObject.transform;

        if(this.GetComponent<AIController>() != null)
        {
            if(this.GetComponent<AIController>().enabled)
            {
                if(networkName != "")
                {
                    playerName.GetComponent<Text>().text = networkName;

                }
                else
                {
                    playerName.GetComponent<Text>().text = aiNames[Random.Range(0, aiNames.Length)];
                }
                
            }
            else
            {
                playerName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");

            }
        }
        
        playerName.GetComponent<NameUIController>().carRend = jeepMesh;  
        
    }

    public void CalculateEngineSound()
    {

        float gearPercentage = (1 / (float) numGears);
        float targetGearFactor = Mathf.InverseLerp(
            gearPercentage * currentGear,
            gearPercentage * (currentGear + 1),
            Mathf.Abs(currentSpeed / maxSpeed)
        );

        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);

        float gearNumFactor = currentGear / (float) numGears;

        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float) numGears) * currentGear;

        if(currentGear > 0 && speedPercentage < downGearMax)
        {
            currentGear--;
        }

        if(speedPercentage > upperGearMax && (currentGear < (numGears - 1)))
        {
            currentGear++;
        }

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);

        highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.25f;

    }


    public void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer * steeringSensitivity, -1, 1) * maxSteerAngle;
        

        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        if(brake != 0)
        {
            brakeLight1.SetActive(true);
            brakeLight2.SetActive(true);  

        }
        else
        {
            brakeLight1.SetActive(false);
            brakeLight2.SetActive(false);  
        }


        float thrustTorque = 0f;
        if(currentSpeed < maxSpeed)
        {
            thrustTorque = accel * torque;
        }
        

        for (int i = 0; i < 4; i++)
        {
            WheelColliders[i].motorTorque = thrustTorque;
            if(i < 2)
            {
                WheelColliders[i].steerAngle = steer;
            }
            else
            {
                WheelColliders[i].brakeTorque = brake;
            }
            
            Quaternion quat;
            Vector3 position;

            WheelColliders[i].GetWorldPose(out position, out quat );
            WheelMeshs[i].transform.position = position;
            WheelMeshs[i].transform.rotation = quat;
            
        }


    }

    public void CheckForSkid()
    {
        int numSkidding = 0;

        for(int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;

            WheelColliders[i].GetGroundHit(out wheelHit);


            if(Mathf.Abs(wheelHit.forwardSlip) >= 0.05f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.15f )
            {
                numSkidding++;

                if(!skidSFX.isPlaying)
                {
                    skidSFX.Play();
                }
                //StartSkidTrail(i);
                skidSmoke[i].transform.position 
                = WheelColliders[i].transform.position - WheelColliders[i].transform.up * WheelColliders[i].radius;
                skidSmoke[i].Emit(1);
            }
            else
            {
                //EndSkidTrail(i);
            }

        }

        if(numSkidding == 0 && skidSFX.isPlaying)
        {
            skidSFX.Stop();
        }

    }



}
