using System;
using Unity.WebRTC;

namespace _WRTC_
{
    partial class WrtcPeer
    {
        [Serializable]
        public class Peer
        {
            public string id;
            public RTCPeerConnection conn;

            public RTCDataChannel
                channel_states,
                channel_flux,
                channel_files,
                channel_vchat;

            //----------------------------------------------------------------------------------------------------------

            public void Init()
            {
                RTCConfiguration config = new()
                {
                    iceServers = new[]
                    {
                        new RTCIceServer
                        {
                            urls = new[] { "stun:stun.l.google.com:19302" },
                        },
                    },
                };

                conn = new RTCPeerConnection(ref config);
            }
        }
    }
}