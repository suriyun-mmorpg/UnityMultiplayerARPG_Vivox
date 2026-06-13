#if UNITY_EDITOR || !UNITY_SERVER
using MultiplayerARPG.MMO;
using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    [RequireComponent(typeof(MMOVivoxTokenProvider))]
    public partial class VivoxManager
    {
    }
}
#endif