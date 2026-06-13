using Cysharp.Threading.Tasks;
using Insthync.DevExtension;
using Insthync.UnityVivoxIntegration;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        [System.Serializable]
        public struct VivoxMessageTypes
        {
            public ushort tokenRequestType;
        }

        [Header("Vivox")]
        public VivoxMessageTypes vivoxMessageTypes = new VivoxMessageTypes()
        {
            tokenRequestType = 1501,
        };

        [DevExtMethods("RegisterMessages")]
        public void RegisterMessages_Vivox()
        {
            RegisterRequestToServer<RequestVivoxTokenMessage, ResponseVivoxTokenMessage>(vivoxMessageTypes.tokenRequestType, HandleRequestVivoxToken);
        }

        public async UniTask<AsyncResponseData<ResponseVivoxTokenMessage>> RequestVivoxToken(RequestVivoxTokenMessage request)
        {
            return await ClientSendRequestAsync<RequestVivoxTokenMessage, ResponseVivoxTokenMessage>(vivoxMessageTypes.tokenRequestType, request);
        }

        protected async UniTaskVoid HandleRequestVivoxToken(
            RequestHandlerData requestHandler, RequestVivoxTokenMessage request,
            RequestProceedResultDelegate<ResponseVivoxTokenMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseVivoxTokenMessage());
                return;
            }
            await VivoxManager.Instance.InitializeForServer();

            string token = null;
            switch (request.action)
            {
                case VivoxAction.Login:
                    token = VivoxManager.Instance.GenerateLoginToken(userId);
                    break;
                case VivoxAction.Join:
                    if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
                    {
                        result.InvokeError(new ResponseVivoxTokenMessage());
                        return;
                    }
                    // Allow to join local voice chat, party voice chat channels only
                    VivoxManager.Instance.GetChannelTypeAndId(request.channelUri, out VivoxChannelType chType, out string chId);
                    switch (chType)
                    {
                        case VivoxChannelType.Positional:
                            if (string.Equals(GetVivoxPositionalChannelId(ChannelId), chId))
                                token = VivoxManager.Instance.GenerateJoinToken(userId, request.channelUri);
                            break;
                        case VivoxChannelType.NonPositional:
                            if (playerCharacter.PartyId > 0 && string.Equals(GetVivoxPartyChannelId(playerCharacter.PartyId), chId))
                                token = VivoxManager.Instance.GenerateJoinToken(userId, request.channelUri);
                            break;
                        case VivoxChannelType.Echo:
                            token = VivoxManager.Instance.GenerateJoinToken(userId, request.channelUri);
                            break;
                    }
                    break;
                    // TODO: Implement other actions
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                result.InvokeError(new ResponseVivoxTokenMessage());
                return;
            }

            result.InvokeSuccess(new ResponseVivoxTokenMessage()
            {
                token = token,
            });
        }

        public static string GetVivoxPositionalChannelId(string channelId)
        {
            return $"POS_{channelId}";
        }

        public static string GetVivoxPartyChannelId(int partyId)
        {
            return $"PTY_{partyId}";
        }
    }
}