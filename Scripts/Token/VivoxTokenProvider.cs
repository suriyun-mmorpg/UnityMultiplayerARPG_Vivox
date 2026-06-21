#if UNITY_EDITOR || !UNITY_SERVER
using Insthync.UnityVivoxIntegration;
using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace MultiplayerARPG
{
    public class VivoxTokenProvider : MonoBehaviour, IVivoxTokenProvider
    {
        BaseGameNetworkManager NetworkManager => BaseGameNetworkManager.Singleton;
        private bool _isAuthorizing = false;
        private bool _isJoiningOrLeavingPositionalChannel = false;
        private bool _isJoiningOrLeavingPartyChannel = false;
        private string _loggedInUserId = string.Empty;
        private string _joinedPositionalChannelId = string.Empty;
        private string _joinedPartyChannelId = string.Empty;
        private string _prevChannelId = string.Empty;
        private int _prevPartyId = 0;

        private void Awake()
        {
            VivoxManager.TokenProvider = this;
        }

        private async void Start()
        {
            await VivoxManager.Instance.InitializeForClient();
        }

        private void OnDestroy()
        {
            VivoxManager.TokenProvider = null;
        }

        private void Update()
        {
            if (_isAuthorizing)
                return;

            if (!VivoxManager.IsLoggedIn)
            {
                if (!string.IsNullOrWhiteSpace(GameInstance.UserId))
                {
                    Login();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(GameInstance.UserId))
                {
                    Logout();
                }
            }

            if (VivoxManager.IsLoggedIn)
            {
                if (GameInstance.PlayingCharacterEntity != null)
                {
                    JoinPartyChannelIfNotJoined();
                    JoinPositionalChannelIfNotJoined();
                    if (!string.IsNullOrWhiteSpace(_joinedPositionalChannelId))
                    {
                        VivoxService.Instance.Set3DPosition(GameInstance.PlayingCharacterEntity.EntityGameObject, _joinedPositionalChannelId);
                    }
                }
                else
                {
                    LeavePartyChannelIfNotLeft();
                    LeavePositionalChannelIfNotLeft();
                }
            }
        }

        private async void LeavePartyChannelIfNotLeft()
        {
            if (_isJoiningOrLeavingPartyChannel)
                return;

            if (!VivoxManager.IsLoggedIn)
                return;

            if (string.IsNullOrWhiteSpace(_joinedPartyChannelId))
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Leaving Party {_joinedPartyChannelId} Start");
#endif
            _isJoiningOrLeavingPartyChannel = true;
            try
            {
                await VivoxService.Instance.LeaveChannelAsync(_joinedPartyChannelId);
                _joinedPartyChannelId = null;
                _prevPartyId = 0;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(ex);
#endif
            }
            _isJoiningOrLeavingPartyChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Leaving Party {_joinedPartyChannelId} End");
#endif
        }

        private async void LeavePositionalChannelIfNotLeft()
        {
            if (_isJoiningOrLeavingPositionalChannel)
                return;

            if (!VivoxManager.IsLoggedIn)
                return;

            if (string.IsNullOrWhiteSpace(_joinedPositionalChannelId))
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Leaving Positional {_joinedPositionalChannelId} Start");
#endif
            _isJoiningOrLeavingPositionalChannel = true;
            try
            {
                await VivoxService.Instance.LeaveChannelAsync(_joinedPositionalChannelId);
                _joinedPositionalChannelId = null;
                _prevChannelId = null;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(ex);
#endif
            }
            _isJoiningOrLeavingPositionalChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Leaving Positional {_joinedPositionalChannelId} End");
#endif
        }

        private async void JoinPartyChannelIfNotJoined()
        {
            if (_isJoiningOrLeavingPartyChannel)
                return;

            if (!VivoxManager.IsLoggedIn)
                return;

            if (!NetworkManager.IsReadyForVivoxConnection)
                return;

            int currentPartyId = GameInstance.PlayingCharacterEntity.PartyId;
            if (_prevPartyId == currentPartyId)
                return;

            if (!string.IsNullOrWhiteSpace(_joinedPartyChannelId))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Vivox Leaving Party {_joinedPartyChannelId} Start");
#endif
                _isJoiningOrLeavingPartyChannel = true;
                try
                {
                    await VivoxService.Instance.LeaveChannelAsync(_joinedPartyChannelId);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError(ex);
#endif
                }
                _isJoiningOrLeavingPartyChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Vivox Leaving Party {_joinedPartyChannelId} End");
#endif
            }

            _joinedPartyChannelId = null;
            if (currentPartyId == 0)
            {
                _prevPartyId = currentPartyId;
                return;
            }

            string joiningChannelId = BaseGameNetworkManager.GetVivoxPartyChannelId(currentPartyId);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Joining Party {joiningChannelId} Start");
#endif
            _isJoiningOrLeavingPartyChannel = true;
            try
            {
                await VivoxService.Instance.JoinGroupChannelAsync(joiningChannelId, ChatCapability.AudioOnly);
                _joinedPartyChannelId = joiningChannelId;
                _prevPartyId = currentPartyId;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(ex);
#endif
                await VivoxService.Instance.LeaveChannelAsync(joiningChannelId);
            }
            _isJoiningOrLeavingPartyChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Joining Party {joiningChannelId} End");
#endif
        }

        private async void JoinPositionalChannelIfNotJoined()
        {
            if (_isJoiningOrLeavingPositionalChannel)
                return;

            if (!VivoxManager.IsLoggedIn)
                return;

            if (!NetworkManager.IsReadyForVivoxConnection)
                return;

            string currentChannelId = NetworkManager.ChannelId;
            if (string.Equals(_prevChannelId, currentChannelId))
                return;

            if (!string.IsNullOrWhiteSpace(_joinedPositionalChannelId))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Vivox Leaving Positional {_joinedPositionalChannelId} Start");
#endif
                _isJoiningOrLeavingPositionalChannel = true;
                try
                {
                    await VivoxService.Instance.LeaveChannelAsync(_joinedPositionalChannelId);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError(ex);
#endif
                }
                _isJoiningOrLeavingPositionalChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Vivox Leaving Positional {_joinedPositionalChannelId} End");
#endif
            }

            _joinedPositionalChannelId = null;
            if (string.IsNullOrWhiteSpace(currentChannelId))
            {
                _prevChannelId = currentChannelId;
                return;
            }

            string joiningChannelId = BaseGameNetworkManager.GetVivoxPositionalChannelId(currentChannelId);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Joining Positional {joiningChannelId} Start");
#endif
            _isJoiningOrLeavingPositionalChannel = true;
            try
            {
                await VivoxService.Instance.JoinPositionalChannelAsync(joiningChannelId, ChatCapability.AudioOnly, new Channel3DProperties());
                _joinedPositionalChannelId = joiningChannelId;
                _prevChannelId = currentChannelId;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(ex);
#endif
                await VivoxService.Instance.LeaveChannelAsync(joiningChannelId);
            }
            _isJoiningOrLeavingPositionalChannel = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Vivox Joining Positional {joiningChannelId} End");
#endif
        }

        private async void Login()
        {
            if (_isAuthorizing)
                return;

            if (VivoxManager.CurrentInitializeState != VivoxManager.InitializeState.Initialized)
                return;

            if (!NetworkManager.IsReadyForVivoxConnection)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Vivox Login Start");
#endif
            _isAuthorizing = true;
            if (!string.IsNullOrWhiteSpace(_loggedInUserId) && !string.Equals(_loggedInUserId, GameInstance.UserId))
            {
                try
                {
                    await VivoxManager.LogoutAsync();
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogException(ex);
#endif
                }
            }
            _prevChannelId = string.Empty;
            _prevPartyId = 0;
            _isJoiningOrLeavingPositionalChannel = false;
            _isJoiningOrLeavingPartyChannel = false;
            _joinedPositionalChannelId = string.Empty;
            _joinedPartyChannelId = string.Empty;
            try
            {
                await VivoxManager.LoginAsync(new LoginOptions()
                {
                    PlayerId = GameInstance.UserId,
                    DisplayName = $"USER_{GameInstance.UserId}",
                });
                _loggedInUserId = GameInstance.UserId;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogException(ex);
#endif
            }
            _isAuthorizing = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Vivox Login End");
#endif
        }

        private async void Logout()
        {
            if (_isAuthorizing)
                return;

            if (!VivoxManager.IsLoggedIn)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Vivox Logout Start");
#endif
            _isAuthorizing = true;
            try
            {
                await VivoxManager.LogoutAsync();
                _loggedInUserId = null;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogException(ex);
#endif
            }
            _isAuthorizing = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Vivox Logout End");
#endif
        }

        public async Task<string> GetTokenAsync(string issuer = null, TimeSpan? expiration = null, string targetUserUri = null, string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
        {
            VivoxAction vivoxAction;
            string channelId;
            bool isLogin = string.Equals(action, "login");
            bool isJoin = string.Equals(action, "join");
            if (isLogin)
            {
                vivoxAction = VivoxAction.Login;
                channelId = string.Empty;
            }
            else if (isJoin)
            {
                vivoxAction = VivoxAction.Join;
                channelId = channelUri;
            }
            else
            {
                return string.Empty;
            }
            var response = await NetworkManager.RequestVivoxToken(new RequestVivoxTokenMessage()
            {
                action = vivoxAction,
                channelUri = channelId,
            });
            if (!response.IsSuccess)
            {
                if (isLogin)
                    NetworkManager.IsReadyForVivoxConnection = false;
                return string.Empty;
            }
            if (isLogin && string.IsNullOrWhiteSpace(response.Response.token))
                NetworkManager.IsReadyForVivoxConnection = false;
            return response.Response.token;
        }
    }
}
#endif