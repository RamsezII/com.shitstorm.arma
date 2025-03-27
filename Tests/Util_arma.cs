using _ARK_;
using _UTIL_;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class Util_arma
{
    public const ushort
        PORT_ARMA = 40000;

    public static readonly string
        URL_ARMA = "https://shitstorm.ovh:" + PORT_ARMA,
        URL_LOCALHOST = "ws://localhost:" + PORT_ARMA;

    enum Codes : byte
    {
        CreateLobby,
    }

    public enum Commands : byte
    {
        CreateLobby = Codes.CreateLobby,
    }

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
        Debug.Log($"[ARMA] Connecting to \"{url}\"...");

        WebSocket websocket = new(url);

        websocket.OnOpen += () =>
        {
            stop = true;
            Debug.Log($"[ARMA_OPEN] Connection open! ({websocket})");

            using BinaryWriter writer = Util.NewWriter();

            writer.Write((byte)operation);
            switch (operation)
            {
                case Commands.CreateLobby:
                    writer.WriteText("Hello, ARMA!");
                    break;

                default:
                    stop = true;
                    Debug.LogWarning($"Unknown code: \"{operation}\"");
                    break;
            }

            websocket.Send(writer.CopyBuffer());
        };

        websocket.OnError += (message) =>
        {
            stop = true;
            Debug.LogWarning($"[ARMA_ERROR] {message}");
        };

        websocket.OnClose += (code) =>
        {
            stop = true;
            Debug.Log($"[ARMA_CLOSE] Connection closed! ({nameof(code)}: {code})");
        };

        websocket.OnMessage += buffer =>
        {
            stop = true;
            Debug.Log($"[ARMA_RESPONSE] OnReceiveBytes! {buffer.Length}");

            using BinaryReader reader = buffer.NewReader();

            switch (operation)
            {
                case Commands.CreateLobby:
                    {
                        string message = reader.ReadText();
                        Debug.Log($"[LOBBY_RESPONSE] {operation}: \"{message}\"");
                    }
                    break;

                default:
                    Debug.LogError($"Unknown code: \"{operation}\"");
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
                websocket.Close();
                NUCLEOR.delegates.onNetworkPull -= websocket.DispatchMessageQueue;
            },
        };
    }
}