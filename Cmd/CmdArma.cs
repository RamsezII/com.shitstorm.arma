using _TERMINAL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _WRTC_
{
    internal partial class CmdArma : IShell
    {
        enum Commands : byte
        {
            CreateLobby,
            ListLobbies,
            JoinLobby,
        }

        IEnumerable<string> IShell.ECommands => Enum.GetNames(typeof(Commands));

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Shell.AddUser(new CmdArma());
        }

        //----------------------------------------------------------------------------------------------------------

        void IShell.OnCmdLine(in string arg0, in LineParser line)
        {
            if (Enum.TryParse(arg0, true, out Commands cmd))
                switch (cmd)
                {
                    case Commands.CreateLobby:
                        WrtcPeer.CmdCreateLobby(line);
                        break;

                    case Commands.ListLobbies:
                        if (line.IsExec)
                            ARMA.RefreshAndLogLobbies();
                        break;

                    default:
                        Debug.LogWarning($"Unimplemented command: \"{arg0}\"");
                        break;
                }
            else
                Debug.LogWarning($"Unknown command: \"{arg0}\"");
        }
    }
}