using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float hudSetting = 0f;


    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        if(PlayerPrefs.HasKey("HUD"))
        {
            hudSetting = PlayerPrefs.GetFloat("HUD");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(RaceMonitor.racing)
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                //Debug.Log("H wa spressed");
                canvasGroup.alpha = canvasGroup.alpha == 1 ? 0 : 1;
                hudSetting = canvasGroup.alpha;
                PlayerPrefs.SetFloat("HUD", canvasGroup.alpha);

            }

        }
        
        
    }
}
