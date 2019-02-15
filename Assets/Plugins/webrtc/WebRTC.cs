using Newtonsoft.Json;
using SimplePeerConnectionM;
using System.Collections.Generic;

namespace WebRTC
{

    public class Signaling
    {
        public delegate void OnData(string s);
        public OnData OnDataMethod;

        public delegate void OnSdp(JSONObject json);
        public OnSdp OnSdpMethod;

        public delegate void OnConnect(JSONObject json);
        public OnConnect OnConnectMethod;

        public PeerConnectionM peer;

        class RoomJson { public string roomId; }

        string roomId;

        public Signaling(string room)
        {
            roomId = room;
            InitPeer();
        }

        void InitPeer()
        {
            List<string> servers = new List<string>();
            servers.Add("stun: stun.skyway.io:3478");
            servers.Add("stun: stun.l.google.com:19302");
            peer = new PeerConnectionM(servers, "", "");
            peer.OnLocalSdpReadytoSend += OnLocalSdpReadytoSend;
            peer.OnIceCandiateReadytoSend += OnIceCandidate;
            peer.AddDataChannel();
            peer.OnLocalDataChannelReady += Connected;
            peer.OnDataFromDataChannelReady += Received;
        }

        class SendSdpJson
        {
            public string sdp;
            public string roomId;
        }

        void OnLocalSdpReadytoSend(int id, string type, string sdp)
        {
            var data = new SendSdpJson();
            data.roomId = roomId;
            data.sdp = type + "%" + sdp;
            var json = new JSONObject(JsonConvert.SerializeObject(data));
            OnSdpMethod(json);
        }

        class SendIce
        {
            public string sdp;
            public string roomId;
        }

        void OnIceCandidate(int id, string candidate, int sdpMlineIndex, string sdpMid)
        {
            var data = new SendIce();
            data.roomId = roomId;
            data.sdp = "ice%" + candidate + "%" + sdpMlineIndex + "%" + sdpMid;
            var json = new JSONObject(JsonConvert.SerializeObject(data));
            OnSdpMethod(json);
        }

        public void Connected(int id)
        {
            var data = new RoomJson();
            data.roomId = roomId;
            roomId = data.roomId;
            var json = new JSONObject(JsonConvert.SerializeObject(data));
            OnConnectMethod(json);
        }

        public void Received(int id, string s)
        {
            OnDataMethod(s);
        }

        class SetSdpJson
        {
            public string sdp;
        }

        public void SetSdp(object raw)
        {
            var data = JsonConvert.DeserializeObject<SetSdpJson>(raw.ToString());
            var arr = data.sdp.Split('%');
            switch (arr[0])
            {
                case "offer":
                    peer.SetRemoteDescription(arr[0], arr[1]);
                    peer.CreateAnswer();
                    break;
                case "answer":
                    peer.SetRemoteDescription(arr[0], arr[1]);
                    break;
                case "ice":
                    peer.AddIceCandidate(arr[1], int.Parse(arr[2]), arr[3]);
                    break;
            }
        }
    }
}
