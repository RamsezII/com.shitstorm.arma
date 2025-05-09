using _ARK_;
using _COBRA_;
using System.Collections.Generic;
using UnityEngine;

namespace _WRTC_
{
    partial class WrtcPeer
    {
        Schedulable hosting_lobby;

        //----------------------------------------------------------------------------------------------------------

        static void InitCmd_host()
        {
            Command.static_domain.AddRoutine(
                "create-public-lobby",
                min_args: 2,
                max_args: 3,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string lobby_name, out _, completions: new string[] { ARMA.settings.lobby_name, }))
                    {
                        exe.args.Add(lobby_name);
                        if (exe.line.TryReadArgument(out string pub_pass, out _, completions: new string[] { ARMA.settings.lobby_pass, }))
                        {
                            exe.args.Add(pub_pass);
                            if (exe.line.TryReadArgument(out string prv_pass, out _))
                                exe.args.Add(prv_pass);
                        }
                    }
                },
                routine: ECmdHost);
        }

        //----------------------------------------------------------------------------------------------------------

        static IEnumerator<CMD_STATUS> ECmdHost(Command.Executor exe)
        {
            Util.InstantiateOrCreateIfAbsent<WrtcPeer>();
            return instance._ECmdHost(exe);
        }

        IEnumerator<CMD_STATUS> _ECmdHost(Command.Executor exe)
        {
            if (hosting_lobby != null && !hosting_lobby.Disposed)
            {
                Debug.LogWarning("Interrupting current network operation:\n" + hosting_lobby.description);
                hosting_lobby.Dispose();
                yield break;
            }

            ARMA.settings.lobby_name = (string)exe.args[0];
            ARMA.settings.lobby_pass = (string)exe.args[1];
            ARMA.SaveSettings(true);

            hosting_lobby = NUCLEOR.instance.subScheduler.AddRoutine(ARMA.EArmaComm(
                ARMA.Commands.CreateLobby,
                onOpenWriter: (_, writer) =>
                {
                    writer.WriteText(ARMA.settings.lobby_name);
                    writer.Write(ARMA.settings.LobbyHash);
                }));

            while (hosting_lobby != null && !hosting_lobby.Disposed)
                yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: hosting_lobby.routine.Current);
        }
    }
}