using _ARK_;
using _UTIL_;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

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

    public static string[] public_lobbies; 

    //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(_PythonBytes))]
    static void _PythonBytes()
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

    public static IEnumerator<float> EArmaComm(Commands operation)
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

            switch (operation)
            {
                case Commands.CreateLobby:
                    writer.WriteText(settings.lobby_name);
                    writer.Write(settings.LobbyHash);
                    break;
            }

            websocket.Send(writer.CopyBuffer());
        };

        websocket.OnError += (message) =>
        {
            stop = true;
            Debug.LogWarning($"[ARMA] {{ {message} }}");
        };

        websocket.OnClose += (code) =>
        {
            stop = true;
            if (code == WebSocketCloseCode.Normal)
                Debug.Log($"[ARMA] Disconnected from \"{url}\"".ToSubLog());
            else
                Debug.LogWarning($"[ARMA] Closed ({nameof(code)}: {code})");
        };

        websocket.OnMessage += buffer =>
        {
            stop = true;
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
                        {
                            stop = false;
                            Debug.Log($"[ARMA] {resp}: {success}");
                        }
                        else
                            Debug.LogWarning($"[ARMA] {resp}: {success}");
                    }
                    break;

                case Codes.ListLobbies:
                    {
                        StringBuilder sb = new();

                        ushort count = reader.ReadUInt16();
                        public_lobbies = new string[count];

                        sb.AppendLine($"[ARMA] {resp}: {count}");

                        for (ushort i = 0; i < count; ++i)
                        {
                            string lobby_name = reader.ReadText();
                            public_lobbies[i] = lobby_name;
                            sb.AppendLine($" . {lobby_name}");
                        }

                        sb.LogAndClear();
                    }
                    break;

                case Codes.Ping:
                    {
                        using BinaryWriter writer = Util.NewWriter();
                        writer.Write(version.VERSION);
                        writer.Write(operation == Commands.CreateLobby && !stop);
                        websocket.Send(writer.CopyBuffer());
                    }
                    break;

                default:
                    Debug.LogError($"[ARMA] Unknown code: \"{resp}\"");
                    break;
            }
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