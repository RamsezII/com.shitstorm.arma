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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            InitCmd_host();
        }

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

        //void IShell.OnCmdLine(in string arg0, in LineParser line)
        //{
        //    if (Enum.TryParse(arg0, true, out Commands cmd))
        //        switch (cmd)
        //        {
        //            case Commands.CreateLobby:
        //                WrtcPeer.CmdCreateLobby(line);
        //                break;

        //            case Commands.ListLobbies:
        //                if (line.IsExec)
        //                    ARMA.RefreshAndLogLobbies();
        //                break;

        //            default:
        //                Debug.LogWarning($"Unimplemented command: \"{arg0}\"");
        //                break;
        //        }
        //    else
        //        Debug.LogWarning($"Unknown command: \"{arg0}\"");
        //}

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