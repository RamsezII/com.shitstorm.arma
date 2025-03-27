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

        WebSocket websocket;

        IEnumerable<string> IShell.ECommands => Enum.GetNames(typeof(Commands));

        BinaryWriter NewWriter() => new(new MemoryStream(), Encoding.UTF8);

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
            websocket = new WebSocket("ws://localhost:3000");

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

            websocket.OnMessage += (bytes) =>
            {
                // getting the message as a string
                string message = Encoding.UTF8.GetString(bytes);
                Debug.Log("OnMessage! " + message);
            };

            // waiting for messages
            websocket.Connect();
        }

        //----------------------------------------------------------------------------------------------------------

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
            if (websocket.State == WebSocketState.Open)
            {
                using BinaryWriter writer = NewWriter();
                writer.Write((byte)Bytes.Bytes);
                writer.Write(bytes);
                Debug.Log($"Sending {writer.GetBuffer().Length} bytes instead of {writer.BaseStream.Position} bytes");
                return;
                websocket.Send(writer.GetBuffer());
            }
            else
                Debug.LogWarning($"Websocket is not open ({websocket.State})");
        }

        void SendText(in string text)
        {
            if (websocket.State == WebSocketState.Open)
            {
                using BinaryWriter writer = NewWriter();
                writer.Write((byte)Bytes.Text);
                writer.WriteText(text);
                Debug.Log($"Sending {writer.GetBuffer().Length} bytes instead of {writer.BaseStream.Position} bytes");
                return;
                websocket.Send(writer.GetBuffer());
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
            websocket?.Close();
        }
    }
}