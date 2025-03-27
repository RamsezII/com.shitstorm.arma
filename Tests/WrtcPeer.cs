using UnityEngine;

namespace _WRTC_
{
    internal class WrtcPeer : MonoBehaviour
    {


        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Util.InstantiateOrCreateIfAbsent<WrtcPeer>();
        }

        //----------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            
        }
    }
}