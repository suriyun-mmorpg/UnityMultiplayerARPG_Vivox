#if UNITY_EDITOR || !UNITY_SERVER
using Cysharp.Threading.Tasks;
using Insthync.UnityVivoxIntegration;
using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
#endif
using UnityEngine;

namespace MultiplayerARPG
{
#if UNITY_EDITOR || !UNITY_SERVER
    public class MMOVivoxPositionalEntity : MonoBehaviour, IVivoxTokenProvider
#else
    public class MMOVivoxPositionalEntity : MonoBehaviour
#endif
    {
#if UNITY_EDITOR || !UNITY_SERVER
        public BaseGameNetworkManager NetworkManager => BaseGameNetworkManager.Singleton;
        private BasePlayerCharacterEntity _entity;
        private BaseVivoxPositionalEntity _positionalEntity;

        private void Awake()
        {
            _entity = GetComponent<BasePlayerCharacterEntity>();
            if (_entity == null)
                enabled = false;
            _entity.onSetOwnerClient += _entity_onSetOwnerClient;
        }

        private async void OnDestroy()
        {
            _entity.onSetOwnerClient -= _entity_onSetOwnerClient;
            VivoxManager.TokenProvider = null;
            if (_entity.IsOwnerClient)
                await _positionalEntity.Logout();
        }

        private void _entity_onSetOwnerClient(BaseGameEntity target)
        {
            if (!_entity.IsOwnerClient)
                return;
            if (_positionalEntity != null)
                return;
            _positionalEntity = gameObject.AddComponent<BaseVivoxPositionalEntity>();
            _positionalEntity.loginOnStart = false;
            RequestChannelIdAndLogin();
        }

        private async void RequestChannelIdAndLogin()
        {
            // Wait until the ID and Character Name updated
            while (string.IsNullOrWhiteSpace(_entity.Id) || string.IsNullOrWhiteSpace(_entity.CharacterName))
            {
                await UniTask.Yield();
            }
            _positionalEntity.displayName = _entity.CharacterName;
            _positionalEntity.playerId = _entity.Id;
            if (NetworkManager != null)
            {
                _positionalEntity.channelName = string.Empty;
                do
                {
                    var response = await NetworkManager.RequestVivoxChannelId(new RequestVivoxChannelIdMessage());
                    _positionalEntity.channelName = response.Response.channelId;
                } while (_positionalEntity.isReconnect && !_positionalEntity.IntendedToLogout && string.IsNullOrWhiteSpace(_positionalEntity.channelName));
                VivoxManager.TokenProvider = this;
#if !UNITY_SERVER
                await _positionalEntity.Login();
#endif
            }
        }

        public async Task<string> GetTokenAsync(string issuer = null, TimeSpan? expiration = null, string targetUserUri = null, string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
        {
            VivoxAction vivoxAction;
            string channelId;
            if (string.Equals(action, "login"))
            {
                vivoxAction = VivoxAction.Login;
                channelId = string.Empty;
            }
            else if (string.Equals(action, "join"))
            {
                vivoxAction = VivoxAction.Join;
                channelId = channelUri;
            }
            else
                return string.Empty;
            var response = await NetworkManager.RequestVivoxToken(new RequestVivoxTokenMessage()
            {
                action = vivoxAction,
                channelUri = channelId,
            });
            if (!response.IsSuccess)
            {
                return string.Empty;
            }
            return response.Response.token;
        }
#endif
    }
}