using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltAIController : MonoBehaviour
{
    public Circuit circuit;
    Vector3 target;
    int CurrentWP = 0;

    float speed = 20.0f;
    public float accuracy = 1.0f;
    public float rotSpeed = 2.0f;


    // Start is called before the first frame update
    void Start()
    {
        target = circuit.waypoints[CurrentWP].transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToTarget = Vector3.Distance(target, this.transform.position);

        Vector3 direction = target - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        this.transform.Translate(0,0,speed * Time.deltaTime);

        if(distanceToTarget < accuracy)
        {
            CurrentWP++;
            if(CurrentWP >= circuit.waypoints.Length)
            {
                CurrentWP = 0;
            }
            target = circuit.waypoints[CurrentWP + 1].transform.position;
            if(CurrentWP >= circuit.waypoints.Length)
            {
                target = circuit.waypoints[0].transform.position;
            }
        }





        
    }
}
