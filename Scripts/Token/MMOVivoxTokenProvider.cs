using Insthync.UnityVivoxIntegration;
using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOVivoxTokenProvider : MonoBehaviour, IVivoxTokenProvider
    {
        MapNetworkManager NetworkManager => BaseGameNetworkManager.Singleton as MapNetworkManager;

        public async Task<string> GetTokenAsync(string issuer = null, TimeSpan? expiration = null, string targetUserUri = null, string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
        {
            VivoxAction vivoxAction;
            if (string.Equals(action, "login"))
                vivoxAction = VivoxAction.Login;
            else if (string.Equals(action, "join"))
                vivoxAction = VivoxAction.Join;
            else
                return string.Empty;
            var response = await NetworkManager.RequestVivoxToken(new RequestVivoxTokenMessage()
            {
                action = vivoxAction,
            });
            if (!response.IsSuccess)
                return string.Empty;
            return response.Response.token;
        }
    }
}