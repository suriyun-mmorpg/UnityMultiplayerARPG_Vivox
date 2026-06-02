using Insthync.UnityVivoxIntegration;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestVivoxTokenMessage : INetSerializable
    {
        public VivoxAction action;
        public string targetUserUri;
        public string channelUri;

        public void Deserialize(NetDataReader reader)
        {
            action = (VivoxAction)reader.GetByte();
            targetUserUri = reader.GetString();
            channelUri = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)action);
            writer.Put(targetUserUri);
            writer.Put(channelUri);
        }
    }
}