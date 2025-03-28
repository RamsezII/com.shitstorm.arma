using System;
using Unity.WebRTC;
using UnityEngine;

namespace _WRTC_
{
    public partial class WrtcPeer : MonoBehaviour
    {
        public static WrtcPeer instance;

        [Serializable]
        public class SignalMessage
        {
            public string type;
            public string name;
            public string target;
            public string from;
            public string sdp;
            public string candidate;
            public string sdpMid;
            public int sdpMLineIndex;
        }

        protected RTCPeerConnection conn;

        //----------------------------------------------------------------------------------------------------------

        protected virtual void Awake()
        {
            instance = this;
        }

        //----------------------------------------------------------------------------------------------------------

        protected virtual void Start()
        {

        }

        //----------------------------------------------------------------------------------------------------------

        protected virtual void OnDestroy()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }

            if (this == instance)
                instance = null;
        }
    }
}