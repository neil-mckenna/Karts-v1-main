using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public float antiRoll = 5000.0f;
    public WheelCollider wheelRFront;
    public WheelCollider wheelLFront;
    public WheelCollider wheelRBack;
    public WheelCollider wheelLBack;

    public GameObject COM;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = COM.transform.localPosition;
        
    }

    void GroundWheels(WheelCollider WR, WheelCollider WL)
    {
        WheelHit hit;
        float travelR = 1.0f;
        float travelL = 1.0f;

        bool groundedR = WR.GetGroundHit(out hit);

        if(groundedR)
        {
            travelR = (-WR.transform.InverseTransformPoint(hit.point).y - WR.radius) / WR.suspensionDistance;
        }

        bool groundedL = WL.GetGroundHit(out hit);

        if(groundedL)
        {
            travelL = (-WL.transform.InverseTransformPoint(hit.point).y - WL.radius) / WL.suspensionDistance;
        }

        float antiRollForce = (travelR - travelL) * antiRoll;

        if(groundedR)
        {
            rb.AddForceAtPosition(WR.transform.up * -antiRollForce, WR.transform.position);
        }

        if(groundedL)
        {
            rb.AddForceAtPosition(WL.transform.up * antiRollForce, WL.transform.position);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundWheels(wheelRFront, wheelLFront);
        GroundWheels(wheelRBack, wheelLBack);
        
    }
}
