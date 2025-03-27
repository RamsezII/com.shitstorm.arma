using _ARK_;
using _TERMINAL_;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace _WRTC_
{
    public class WrtcPeer : MonoBehaviour, IShell
    {
        public enum Commands : byte
        {
            WrtcInit,
            WrtcSendBytes,
            WrtcSendText,
        }

        public enum Bytes : byte
        {
            Bytes,
            Text,
        }

        const ushort PORT_ARMA = 40000;
        const string EVE_DOMAIN = "shitstorm.ovh";

        static readonly string URL_ARMA = "https://shitstorm.ovh:" + PORT_ARMA;
        static readonly string URL_LOCALHOST = "ws://localhost:" + PORT_ARMA;

        IEnumerable<string> IShell.ECommands => Enum.GetNames(typeof(Commands));

        BinaryWriter NewWriter() => new(new MemoryStream(), Encoding.UTF8);

        WebSocket websocket;

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(_PythonBytes))]
        static void _PythonBytes()
        {
            StringBuilder sb = new();

            sb.AppendLine($"class {nameof(Bytes)}(Enum):");
            foreach (Bytes code in Enum.GetValues(typeof(Bytes)))
                sb.AppendLine($"\t{code} = {(int)code}");

            string log = sb.TroncatedForLog();
            Debug.Log(log);
            GUIUtility.systemCopyBuffer = log;
        }
#endif

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Util.InstantiateOrCreateIfAbsent<WrtcPeer>();
        }

        //----------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            Shell.AddUser(this);
            websocket?.Close();
            websocket = null;
        }

        //----------------------------------------------------------------------------------------------------------

        void Init()
        {
            websocket = new WebSocket(URL_LOCALHOST);

            websocket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
            };

            websocket.OnError += (message) =>
            {
                Debug.Log("Error! " + message);
            };

            websocket.OnClose += (code) =>
            {
                Debug.Log($"Connection closed! ({nameof(code)}: {code})");
            };

            websocket.OnMessage += OnReceiveBytes;

            // waiting for messages
            websocket.Connect();

#if !UNITY_WEBGL || UNITY_EDITOR
            NUCLEOR.delegates.onNetworkPull += websocket.DispatchMessageQueue;
#endif
        }

        //----------------------------------------------------------------------------------------------------------

        void OnReceiveBytes(byte[] bytes)
        {
            Debug.Log("OnReceiveBytes! " + bytes.Length);
            using BinaryReader reader = new(new MemoryStream(bytes), Encoding.UTF8);

            Bytes code = (Bytes)reader.ReadByte();
            switch (code)
            {
                case Bytes.Text:
                    {
                        string message = reader.ReadText();
                        Debug.Log("OnMessage! " + message);
                    }
                    break;

                default:
                    Debug.LogError($"Unknown code: \"{code}\"");
                    break;
            }
        }

        void CmdSendBytes(in LineParser line)
        {
            List<byte> bytes = new();

            while (line.TryRead(out string read))
            {
                if (byte.TryParse(read, out byte b))
                    bytes.Add(b);
                else
                    Debug.LogError($"Failed to parse byte: \"{read}\"");
            }

            if (bytes.Count > 0)
                SendBytes(bytes.ToArray());
            else
                Debug.LogWarning("No bytes to send");
        }

        void SendBytes(params byte[] bytes)
        {
            if (websocket.State == WebSocketState.Open || true)
            {
                using BinaryWriter writer = NewWriter();

                writer.Write((byte)Bytes.Bytes);
                writer.Write(bytes);

                websocket.Send(writer.CopyBytes());
            }
            else
                Debug.LogWarning($"Websocket is not open ({websocket.State})");
        }

        void SendText(in string text)
        {
            if (websocket.State == WebSocketState.Open || true)
            {
                using BinaryWriter writer = NewWriter();

                writer.Write((byte)Bytes.Text);
                writer.WriteText(text);

                websocket.Send(writer.CopyBytes());
            }
            else
                Debug.LogWarning($"Websocket is not open ({websocket.State})");
        }

        void IShell.OnCmdLine(in string arg0, in LineParser line)
        {
            if (Enum.TryParse(arg0, out Commands code))
                switch (code)
                {
                    case Commands.WrtcInit:
                        if (line.IsExec)
                            Init();
                        break;

                    case Commands.WrtcSendBytes:
                        CmdSendBytes(line);
                        break;

                    case Commands.WrtcSendText:
                        {
                            if (line.TryRead(out string read))
                                if (line.IsExec)
                                    SendText(read);
                        }
                        break;

                    default:
                        Debug.LogError($"Unimplemented command: \"{code}\"");
                        break;
                }
            else
                Debug.LogError($"Unknown command: \"{arg0}\"");
        }

        //----------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            Shell.RemoveUser(this);
            if (websocket != null)
            {
                NUCLEOR.delegates.onNetworkPull -= websocket.DispatchMessageQueue;
                websocket.Close();
            }
        }
    }
}