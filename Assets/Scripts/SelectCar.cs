using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCar : MonoBehaviour
{
    public GameObject[] cars;
    int currentCar = 0; 

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerCar"))
        {
            currentCar = PlayerPrefs.GetInt("PlayerCar");
        }
        this.transform.LookAt(cars[currentCar].transform.position);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentCar++;
            if(currentCar > cars.Length - 1)
            {
                currentCar = 0;
            }

            PlayerPrefs.SetInt("PlayerCar", currentCar);

            Quaternion lookDir = Quaternion.LookRotation(cars[currentCar].transform.position - this.transform.position);
            //transform.rotation = Quaternion.Lerp(transform.rotation, lookDir, Time.deltaTime);
            this.transform.rotation = lookDir;
            
        }

        
    }
}
