using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseVivoxChannelIdMessage : INetSerializable
    {
        public string channelId;

        public void Deserialize(NetDataReader reader)
        {
            channelId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(channelId);
        }
    }
}