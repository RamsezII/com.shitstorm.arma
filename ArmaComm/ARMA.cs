using _ARK_;
using _UTIL_;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace _WRTC_
{
    public static partial class ARMA
    {
        public const ushort
            PORT_ARMA = 40000;

        public static readonly string
            URL_ARMA = "https://shitstorm.ovh:" + PORT_ARMA,
            URL_LOCALHOST = "ws://localhost:" + PORT_ARMA;

        enum Codes : byte
        {
            WrongVersion,
            CreateLobby,
            ListLobbies,
            JoinLobby,
            Ping,
        }

        public enum Commands : byte
        {
            CreateLobby = Codes.CreateLobby,
            ListLobbies = Codes.ListLobbies,
            JoinLobby = Codes.JoinLobby,
        }

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            public_lobbies.ClearList();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            InitTexts();
            InitList();
        }

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(ArmaCodesToPython))]
        static void ArmaCodesToPython()
        {
            StringBuilder sb = new();

            sb.AppendLine($"class {nameof(Codes)}(Enum):");
            foreach (Codes code in Enum.GetValues(typeof(Codes)))
                sb.AppendLine($"\t{code} = {(int)code}");

            string log = sb.TroncatedForLog();
            Debug.Log(log);
            GUIUtility.systemCopyBuffer = log;
        }
#endif

        //----------------------------------------------------------------------------------------------------------

        public static IEnumerator<float> EArmaComm(Commands operation, Action<WebSocket, BinaryWriter> onOpenWriter = null, Func<WebSocket, BinaryReader, bool> onMessage = null, Action<string> onError = null, Action<WebSocketCloseCode> onClose = null)
        {
            bool stop = false;

            string url = URL_LOCALHOST;
            Debug.Log($"[ARMA] Connecting to \"{url}\"...".ToSubLog());

            WebSocket websocket = new(url);

            websocket.OnOpen += () =>
            {
                Debug.Log($"[ARMA] Connected to \"{url}\"".ToSubLog());

                using BinaryWriter writer = Util.NewWriter();

                writer.Write(version.VERSION);
                writer.Write((byte)operation);

                onOpenWriter?.Invoke(websocket, writer);

                websocket.Send(writer.CopyBuffer());
            };

            websocket.OnError += (message) =>
            {
                stop = true;
                Debug.LogWarning($"[ARMA] {{ {message} }}");

                switch (operation)
                {
                    case Commands.ListLobbies:
                        public_lobbies.ClearList();
                        break;
                }

                onError?.Invoke(message);
            };

            websocket.OnClose += (code) =>
            {
                stop = true;
                if (code == WebSocketCloseCode.Normal)
                    Debug.Log($"[ARMA] Disconnected from \"{url}\"".ToSubLog());
                else
                    Debug.LogWarning($"[ARMA] Closed ({nameof(code)}: {code})");
                onClose?.Invoke(code);
            };

            websocket.OnMessage += buffer =>
            {
                using BinaryReader reader = buffer.NewReader();

                Codes resp = (Codes)reader.ReadByte();
                switch (resp)
                {
                    case Codes.WrongVersion:
                        Debug.LogWarning($"[ARMA] {resp}");
                        break;

                    case Codes.CreateLobby:
                        {
                            bool success = reader.ReadBoolean();
                            if (success)
                                Debug.Log($"[ARMA] {resp}: {success}");
                            else
                                Debug.LogWarning($"[ARMA] {resp}: {success}");
                        }
                        break;

                    case Codes.ListLobbies:
                        public_lobbies.ModifyList(list =>
                        {
                            list.Clear();
                            ushort count = reader.ReadUInt16();
                            for (ushort i = 0; i < count; ++i)
                                list.Add(reader.ReadText());
                        });
                        break;

                    case Codes.Ping:
                        {
                            using BinaryWriter writer = Util.NewWriter();
                            writer.Write(version.VERSION);
                            writer.Write(operation == Commands.CreateLobby && !stop);
                            websocket.Send(writer.CopyBuffer());
                        }
                        break;
                }

                stop = onMessage == null || !onMessage(websocket, reader);
            };

            websocket.Connect();

            NUCLEOR.delegates.onNetworkPull += websocket.DispatchMessageQueue;

            while (!stop)
                yield return 0;

            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    Debug.Log($"[ARMA] Disposed connection \"{url}\" ({nameof(stop)}: {stop})".ToSubLog());
                    websocket.Close();
                    NUCLEOR.delegates.onNetworkPull -= websocket.DispatchMessageQueue;
                },
            };
        }
    }
}