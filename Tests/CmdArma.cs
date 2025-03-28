using _ARK_;
using _TERMINAL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _WRTC_
{
    internal class CmdArma : IShell
    {
        enum Commands : byte
        {
            CreateLobby,
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
                        if (line.IsExec)
                            NUCLEOR.instance.scheduler.AddRoutine(ARMA.EArmaComm(ARMA.Commands.CreateLobby));
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