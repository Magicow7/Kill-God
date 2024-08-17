using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public GameObject player;
    [Space]
    public Transform spawnPoint;
    public MapGenerator mapGenerator;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster(){
        base.OnConnectedToMaster();

        Debug.Log("Connected to Server.");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby(){
        base.OnJoinedLobby();

        Debug.Log("Joined Lobby.");

        PhotonNetwork.JoinOrCreateRoom("test", null, null);

        

        
    }

    public override void OnJoinedRoom(){
        base.OnJoinedRoom();

        Debug.Log("We are connected and in a room now");

        GameObject newPlayer = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity); 
        newPlayer.GetComponent<PlayerSetup>().IsLocalPlayer();   
    }
    
}
