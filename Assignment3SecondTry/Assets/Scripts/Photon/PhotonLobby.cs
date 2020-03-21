using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby lobby;
    //RoomInfo[] rooms;
    public GameObject battleButton;
    public GameObject cancelButton;

    private void Awake()
    {
        lobby = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();//connect to master server
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has connect to the Photon master server");
        PhotonNetwork.AutomaticallySyncScene = true;
        battleButton.SetActive(true);//enable button to allow to join the game
    }

    public void OnBattleButtonClicked()
    {
        battleButton.SetActive(false);
        cancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join a ramdom but failed due to: " + message);
        CreateRoom();

    }

    public void CreateRoom()
    {
        int randomRoomName = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true,
            MaxPlayers = (byte)MultiplayerSetting.multiplayerSetting.maxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomName, roomOps);
        Debug.Log("Creating a room");
    }


    public override void OnJoinedRoom()
    {
        Debug.Log("We are now in a room");
    }



    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create room failed because there must already be a room with the same name");
        CreateRoom();
    }

    public void OnCancelButtonClicked()
    {
        battleButton.SetActive(true);
        cancelButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }
}
