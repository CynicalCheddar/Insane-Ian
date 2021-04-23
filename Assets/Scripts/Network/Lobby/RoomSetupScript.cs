﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using TMPro;

public class RoomSetupScript : MonoBehaviourPunCallbacks
{
    private int maxPlayers = 2;
    private string roomName = "myRoom";
    public string mainLobbySceneName = "";
    List<RoomInfo> createdRooms = new List<RoomInfo>();
    public TMP_InputField usernameInputField;
    public TextMeshProUGUI observedMaxPlayersText;
    public TMP_InputField roomNameInputField;
    public Button hostGameButton;
    public TextMeshProUGUI hostGameButtonText;
    string gameVersion = "0.9";
    public Text roomNameInUseText;
    private void Start()
    {
        //Random rnd = new Random();
        //roomName = roomName + rnd.Next(1,9999).ToString();
        
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = "host";
        }

        if (PhotonNetwork.InLobby) {
            EnableHostGameButton();
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log("OnConnectedToMaster");
        //After we connected to Master server, join the Lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    void EnableHostGameButton() {
        hostGameButtonText.text = "Host Game";
        hostGameButton.interactable = true;
    }

    public override void OnJoinedLobby() {
        EnableHostGameButton();
    }

    public void SetMaxPlayers(int newMaxPlayers)
    {
        maxPlayers = newMaxPlayers;
    }

    public void SetRoomName(string newRoomName) {
        roomName = newRoomName;
    }
    // Start is called before the first frame update
    public void CreateRoomWithSettings()
    {
        // ideally, observe the text value of the max players
        if (observedMaxPlayersText) SetMaxPlayers(Int32.Parse(observedMaxPlayersText.text));

        if (roomNameInputField.text != "") SetRoomName(roomNameInputField.text);
        // now we have got the settings we need, create the room and load the main lobby scene
        RoomOptions roomOptions = new RoomOptions();
        
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte) maxPlayers; //Set any number
        if (usernameInputField.text != "") PhotonNetwork.NickName = usernameInputField.text;
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        // load the lobby scene for the room
      
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        //After this callback, update the room list
        createdRooms = roomList;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        roomNameInUseText.gameObject.SetActive(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public override void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
        //Load the Scene called GameLevel (Make sure it's added to build settings)
        PhotonNetwork.LoadLevel(mainLobbySceneName);
    }
}
