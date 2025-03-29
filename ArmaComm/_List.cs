using _ARK_;
using _UTIL_;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _WRTC_
{
    partial class ARMA
    {
        public static readonly ListListener lobbies_list_users = new();
        public static readonly ListListener<string> public_lobbies = new();

        static Schedulable operation_refresh_lobbies;

        //----------------------------------------------------------------------------------------------------------

        public static void RefreshAndLogLobbies()
        {
            object logLobbies = new();
            public_lobbies.AddOneTimeListener(_ =>
            {
                LogPublicLobbies();
                lobbies_list_users.RemoveElement(logLobbies);
            });
            lobbies_list_users.AddElement(logLobbies);
        }

        public static void LogPublicLobbies()
        {
            StringBuilder sb = new();
            sb.AppendLine($"[ARMA] Public lobbies: {public_lobbies._list.Count}");
            for (int i = 0; i < public_lobbies._list.Count; ++i)
                sb.AppendLine($" . {public_lobbies._list[i]}");
            sb.LogAndClear();
        }

        static void InitList()
        {
            lobbies_list_users.ClearList();
            lobbies_list_users.AddListener1(toggle =>
            {
                if (operation_refresh_lobbies != null)
                {
                    if (!operation_refresh_lobbies.Disposed)
                    {
                        Debug.Log($"[ARMA] Interrupting {nameof(operation_refresh_lobbies)}".ToSubLog());
                        operation_refresh_lobbies.Dispose();
                    }
                    operation_refresh_lobbies = null;
                }

                if (toggle)
                {
                    operation_refresh_lobbies = NUCLEOR.instance.subScheduler.AddRoutine(ERefreshList());

                    static IEnumerator<float> ERefreshList()
                    {
                        while (operation_refresh_lobbies != null && !operation_refresh_lobbies.Disposed)
                        {
                            using Schedulable eArmaComm = NUCLEOR.instance.subScheduler.AddRoutine(EArmaComm(Commands.ListLobbies, onClose: closeCode => operation_refresh_lobbies?.Dispose()));

                            while (eArmaComm != null && !eArmaComm.Disposed)
                                yield return eArmaComm.routine.Current;

                            const float refresh_timer = 1.5f;
                            float timer = 0;

                            while (timer < refresh_timer)
                            {
                                timer += Time.unscaledDeltaTime;
                                yield return timer / refresh_timer;
                            }

                            yield return 0;
                        }
                    }
                }
            });
        }
    }
}