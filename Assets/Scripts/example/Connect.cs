using Newtonsoft.Json;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebRTC;

public class Connect : MonoBehaviour
{
    public SocketIOComponent socket;
    Signaling signaling;
    public string roomId = "test2";

    void Start()
    {
        signaling = new Signaling(roomId);
        signaling.OnConnectMethod += OnConnet;
        signaling.OnDataMethod += OnData;
        signaling.OnSdpMethod += OnSdp;
    }

    void OnConnet(JSONObject obj)
    {
        Debug.Log("connect");
        socket.Emit("connect", obj);
        signaling.peer.SendDataViaDataChannel("test");
    }

    public void Send(string str)
    {
        signaling.peer.SendDataViaDataChannel(str);
    }

    void OnData(string s)
    {
        Debug.Log("data " + s);
    }

    void OnSdp(JSONObject obj)
    {
        Debug.Log("sendsdp " + obj.ToString());
        socket.Emit("sdp", obj);
    }

    class RoomJson { public string roomId; }

    public void Create()
    {
        Debug.Log("create");
        var data = new RoomJson();
        data.roomId = roomId;
        var json = new JSONObject(JsonConvert.SerializeObject(data));
        socket.Emit("create", json);
        socket.On("sdp", OnSdpData);
    }

    void OnSdpData(SocketIOEvent e)
    {
        Debug.Log("onsdp " + e.data.ToString());
        signaling.SetSdp(e.data);
    }

    public void Join()
    {
        Debug.Log("join");
        var data = new RoomJson();
        data.roomId = roomId;
        var json = new JSONObject(JsonConvert.SerializeObject(data));
        socket.Emit("join", json);
        socket.On("join", OnJoin);
        socket.On("sdp", OnSdpData);
    }

    void OnJoin(SocketIOEvent e)
    {
        Debug.Log("onjoin");
        signaling.peer.CreateOffer();
    }

}
