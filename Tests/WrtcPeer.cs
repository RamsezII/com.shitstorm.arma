using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

namespace _WRTC_
{
    internal class WrtcPeer : MonoBehaviour
    {
        RTCPeerConnection conn;

        RTCDataChannel
            channel_states,
            channel_flux,
            channel_audio;

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Util.InstantiateOrCreateIfAbsent<WrtcPeer>();
        }

        //----------------------------------------------------------------------------------------------------------

        private void Awake()
        {

        }

        //----------------------------------------------------------------------------------------------------------

        IEnumerator<float> EInit()
        {
            conn = new();

            channel_states = conn.CreateDataChannel("data");
            channel_states.OnOpen += () => Debug.Log("Channel opened");
            channel_states.OnClose += () => Debug.Log("Channel closed");

            while (conn.ConnectionState != RTCPeerConnectionState.New)
                yield return 0;
        }

        //----------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            if (channel_states != null)
            {
                channel_states.Close();
                channel_states.Dispose();
            }

            if (channel_flux != null)
            {
                channel_flux.Close();
                channel_flux.Dispose();
            }

            if (channel_audio != null)
            {
                channel_audio.Close();
                channel_audio.Dispose();
            }

            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}