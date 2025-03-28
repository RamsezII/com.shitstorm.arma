using _ARK_;
using _TERMINAL_;
using UnityEngine;

namespace _WRTC_
{
    partial class WrtcPeer
    {
        Schedulable hosting_lobby;

        //----------------------------------------------------------------------------------------------------------

        public static void CmdCreateLobby(in LineParser line)
        {
            string lobby_name = line.Read();
            if (line.IsCplThis)
                line.OnCpls(lobby_name, ARMA.settings.lobby_name);

            string lobby_pass = line.Read();

            if (line.IsExec)
            {
                Util.InstantiateOrCreateIfAbsent<WrtcPeer>();
                instance.CreateLobby(lobby_name, lobby_pass);
            }
        }

        public void CreateLobby(in string lobby_name, in string lobby_pass)
        {
            if (hosting_lobby != null && !hosting_lobby.Disposed)
            {
                Debug.LogWarning("Interrupting current network operation:\n" + hosting_lobby.description);
                hosting_lobby.Dispose();
                return;
            }

            ARMA.settings.lobby_pass = lobby_pass;

            if (!string.IsNullOrWhiteSpace(lobby_name))
            {
                ARMA.settings.lobby_name = lobby_name;
                ARMA.SaveSettings(true);
            }

            hosting_lobby = NUCLEOR.instance.subScheduler.AddRoutine(ARMA.EArmaComm(ARMA.Commands.CreateLobby));
        }
    }
}