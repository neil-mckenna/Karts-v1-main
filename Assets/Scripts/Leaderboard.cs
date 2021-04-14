using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

struct PlayerStats
{
    public string name;
    public int position;
    public float time;
    
    public PlayerStats(string n, int p, float t)
    {
        name = n;
        position = p;
        time = t;
    }
}

public class Leaderboard
{
    static Dictionary<int, PlayerStats> lb = new Dictionary<int, PlayerStats>();
    static int carsRegistered = -1;

    public static void Reset() 
    {
        lb.Clear();
        carsRegistered = -1;
        
    }

    public static int RegisterCar(string name) {

        carsRegistered++;
        lb.Add(carsRegistered, new PlayerStats(name, 0, 0.0f));
        return carsRegistered;
    }

    public static void SetPosition(int rego, int lap, int checkpoint, float time)
    {
        int position = lap * 1000 + checkpoint;
        lb[rego] = new PlayerStats(lb[rego].name, position, time);   
    }

    public static string GetPosition(int rego)
    {
        int index = 0;

        foreach (KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.time))
        {
            index++;
            if(pos.Key == rego)
            {
                switch(index)
                {
                    case 1: return "1st";
                    case 2: return "2nd";
                    case 3: return "3rd";
                    case 4: return "4th";
                    case 5: return "5th";
                    case 6: return "6th";
                    case 7: return "7th";
                    case 8: return "8th";
                    case 9: return "9th";
                    case 10: return "10th";
                    case 11: return "11th";
                    case 12: return "12th";
                    case 13: return "13th";
                    case 14: return "14th";
                    case 15: return "15th";
                    case 16: return "16th";
                    case 17: return "17th";
                    case 18: return "18th";
                    case 19: return "19th";
                    case 20: return "20th";
                }
            }
        }
        return "Unknown"; 

    }

    public static List<string> GetPlaces() {

        List<string> places = new List<string>();

        foreach (KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.time)) {

            places.Add(pos.Value.name);
        }
        return places;
    }
    
}
