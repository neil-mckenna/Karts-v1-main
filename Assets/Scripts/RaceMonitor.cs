using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class RaceMonitor : MonoBehaviourPunCallbacks
{

    public GameObject[] countDownItems;
    CheckpointManager[] carsCPM;

    public GameObject[] singleCarPrefabs;
    public GameObject[] carPrefabs;
    public Transform[] spawnPos;

    public static bool racing = false;
    public static int totalLaps = 3;
    public GameObject gameOverPanel;
    public GameObject HUD;

    public GameObject startRace;
    public GameObject waitingText;

    int playerCar;
    GameObject pcar;
    Vector3 startPos = new Vector3(0,0,0);
    Quaternion startRot = new Quaternion(0,0,0,1);

    // Start is called before the first frame update
    void Start()
    {
        racing = false;

        foreach(GameObject g in countDownItems)
        {
            g.SetActive(false);
        }

        gameOverPanel.SetActive(false);
        startRace.SetActive(false);
        waitingText.SetActive(false);

        if(PlayerPrefs.HasKey("PlayerCar"))
        {
            playerCar = PlayerPrefs.GetInt("PlayerCar");
        }
        else
        {
            playerCar = 1;
        }
        
        
        int randomStartPos = Random.Range(0, spawnPos.Length);
        startPos = spawnPos[randomStartPos].position;
        startRot = spawnPos[randomStartPos].rotation;
        


        if(PhotonNetwork.IsConnected)// multiplayer
        {
            startPos = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].position;
            startRot = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation;

            if(NetworkedPlayer.LocalPlayerInstance == null)
            {
                Debug.Log("player car prefab " + carPrefabs[playerCar].name + " /pos " + startPos + "/rot " + startRot);
                pcar = PhotonNetwork.Instantiate(carPrefabs[playerCar].name, startPos, startRot, 0);
                Debug.Log("car instantiated " + pcar);
                CameraPlayerSetup(pcar);

            }

            if(PhotonNetwork.IsMasterClient)
            {
                startRace.SetActive(true);
            }
            else
            {
                waitingText.SetActive(true);
            } 
        }
        else // Singleplayer
        {
            pcar = Instantiate(singleCarPrefabs[playerCar]);        
            pcar.transform.position = startPos;
            pcar.transform.rotation = startRot;

            foreach(Transform t in spawnPos)
            {
                if(t == spawnPos[randomStartPos]) continue;
                GameObject car = Instantiate(singleCarPrefabs[Random.Range(0, singleCarPrefabs.Length)]);
                if(!car.activeSelf)
                {
                    car.SetActive(true);
                }
                car.transform.position = t.position;
                car.transform.rotation = t.rotation;
            }

            
            Invoke("StartGame", 2f);
            CameraPlayerSetup(pcar);
        } 
    }

    public void CameraPlayerSetup(GameObject pcar)
    {
        SmoothFollow.playerCar = pcar.gameObject.GetComponent<Drive>().rb.transform;
        if (!pcar.activeSelf)
        {
            pcar.SetActive(true);
        }
        pcar.GetComponent<AIController>().enabled = false;
        pcar.GetComponent<Drive>().enabled = true;
        pcar.GetComponent<PlayerController>().enabled = true;

    }

    public void BeginGame()
    {
        string[] aiNames = { "Amy", "Sheldon", "Howard", "Penny", "Leonard", "Raj", "Bernadette", "Stuart", "Leslie", "Bert", "Barry", "Priya", "Emily", "Wheaton" };
        
        // spawn Ai 
        for(int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            startPos = spawnPos[i].position;
            startRot = spawnPos[i].rotation;
            int r = Random.Range(0, carPrefabs.Length);

            object[] instanceData = new object[1];
            instanceData[0] = (string) aiNames[Random.Range(0, aiNames.Length)];

            GameObject AICar = PhotonNetwork.Instantiate(carPrefabs[r].name, startPos, startRot, 0, instanceData);
            AICar.GetComponent<AIController>().enabled = true;
            AICar.GetComponent<Drive>().enabled = true;
            AICar.GetComponent<Drive>().networkName = (string) instanceData[0];
            AICar.GetComponent<PlayerController>().enabled = false;

        }

        startPos = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].position;
        startRot = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation;

        if (NetworkedPlayer.LocalPlayerInstance == null)
        {
            Debug.Log("player car prefab " + carPrefabs[playerCar].name + " /pos " + startPos + "/rot " + startRot);
            pcar = PhotonNetwork.Instantiate(carPrefabs[playerCar].name, startPos, startRot, 0);
            Debug.Log("car instantiated " + pcar);
            CameraPlayerSetup(pcar);

        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All, null);
        }
    }

    [PunRPC]
    public void StartGame()
    {
        StartCoroutine(nameof(PlayCountDown));
        startRace.SetActive(false);
        waitingText.SetActive(false);

        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
        carsCPM = new CheckpointManager[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            carsCPM[i] = cars[i].GetComponent<CheckpointManager>();
        }

    }

    public IEnumerator PlayCountDown()
    {
        yield return new WaitForSeconds(2f);
        foreach(GameObject g in countDownItems)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(1f);
            g.SetActive(false);
        }
        racing = true;

    }

    [PunRPC]
    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("Track1");
    }

    public void RestartLevel()
    {
        racing = false;
        if(PhotonNetwork.IsConnected)
        {
            photonView.RPC("RestartGame", RpcTarget.All, null);
        }
        else
        {
            SceneManager.LoadScene("Track1");
        }
        

    }

    // bool raceOver = false;
    // private void Update() {
    //     if(Input.GetKeyDown(KeyCode.R))
    //     {
    //         raceOver = true;
    //     }
    // }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!racing) { return; }
        int finishedCount = 0;

        foreach(CheckpointManager cpm in carsCPM)
        {
            if(cpm.lap == totalLaps + 1)
            {
                finishedCount++;
            }
            if(finishedCount == carsCPM.Length)
            {
                HUD.SetActive(false);
                gameOverPanel.SetActive(true);
            }
        }

    }

    public void LoadMainMenu()
    {
        Debug.LogWarning("Loading Main Menu");
        racing = false;
        
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

        }

        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Debug.LogWarning("Quitting Game");
        Application.Quit();
    }
}
