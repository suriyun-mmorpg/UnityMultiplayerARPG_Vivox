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
            public ushort channelIdRequestType;
            public ushort tokenRequestType;
        }

        [Header("Vivox")]
        public VivoxMessageTypes vivoxMessageTypes = new VivoxMessageTypes()
        {
            channelIdRequestType = 1500,
            tokenRequestType = 1501,
        };

        [DevExtMethods("RegisterMessages")]
        public void RegisterMessages_Vivox()
        {
            RegisterRequestToServer<RequestVivoxChannelIdMessage, ResponseVivoxChannelIdMessage>(vivoxMessageTypes.channelIdRequestType, HandleRequestVivoxChannelId);
            RegisterRequestToServer<RequestVivoxTokenMessage, ResponseVivoxTokenMessage>(vivoxMessageTypes.tokenRequestType, HandleRequestVivoxToken);
        }

        public async UniTask<AsyncResponseData<ResponseVivoxChannelIdMessage>> RequestVivoxChannelId(RequestVivoxChannelIdMessage request)
        {
            return await ClientSendRequestAsync<RequestVivoxChannelIdMessage, ResponseVivoxChannelIdMessage>(vivoxMessageTypes.channelIdRequestType, request);
        }

        public async UniTask<AsyncResponseData<ResponseVivoxTokenMessage>> RequestVivoxToken(RequestVivoxTokenMessage request)
        {
            return await ClientSendRequestAsync<RequestVivoxTokenMessage, ResponseVivoxTokenMessage>(vivoxMessageTypes.tokenRequestType, request);
        }

        protected UniTaskVoid HandleRequestVivoxChannelId(
            RequestHandlerData requestHandler, RequestVivoxChannelIdMessage request,
            RequestProceedResultDelegate<ResponseVivoxChannelIdMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseVivoxChannelIdMessage());
                return default;
            }

            result.InvokeSuccess(new ResponseVivoxChannelIdMessage()
            {
                channelId = ChannelId,
            });
            return default;
        }

        protected async UniTaskVoid HandleRequestVivoxToken(
            RequestHandlerData requestHandler, RequestVivoxTokenMessage request,
            RequestProceedResultDelegate<ResponseVivoxTokenMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseVivoxTokenMessage());
            }
            await VivoxManager.Instance.InitializeForServer();

            string token = null;
            switch (request.action)
            {
                case VivoxAction.Login:
                    token = VivoxManager.Instance.GenerateLoginToken(playerCharacter.Id);
                    break;
                case VivoxAction.Join:
                    // Allow to join local voice chat, party voice chat channels only
                    VivoxManager.Instance.GetChannelTypeAndId(request.channelUri, out VivoxChannelType chType, out string chId);
                    switch (chType)
                    {
                        case VivoxChannelType.Positional:
                            if (string.Equals(ChannelId, chId))
                                token = VivoxManager.Instance.GenerateJoinToken(playerCharacter.Id, request.channelUri);
                            break;
                        case VivoxChannelType.NonPositional:
                            if (playerCharacter.PartyId > 0 && string.Equals($"PARTY_{playerCharacter.PartyId}", request.channelUri))
                                token = VivoxManager.Instance.GenerateJoinToken(playerCharacter.Id, request.channelUri);
                            break;
                        case VivoxChannelType.Echo:
                            token = VivoxManager.Instance.GenerateJoinToken(playerCharacter.Id, request.channelUri);
                            break;
                    }
                    break;
                    // TODO: Implement other actions
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                result.InvokeError(new ResponseVivoxTokenMessage());
            }

            result.InvokeSuccess(new ResponseVivoxTokenMessage()
            {
                token = token,
            });
        }
    }
}