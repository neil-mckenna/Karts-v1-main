using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayLeaderboard : MonoBehaviour
{
    public Text first;
    public Text second;
    public Text third;
    public Text fourth;

    void Start() 
    {
        Leaderboard.Reset();
    }
    
    void LateUpdate() 
    {

        List<string> places = Leaderboard.GetPlaces();
        //Debug.LogWarning(places);

        if(places.Count > 0)
        {
            first.text = places[0];

        }
        if(places.Count > 1)
        {
            second.text = places[1];
        }
        if(places.Count > 2)
        {
            third.text = places[2];
        }
        if(places.Count > 3)
        {
            fourth.text = places[3];
        }
        
        
    }


    
}
