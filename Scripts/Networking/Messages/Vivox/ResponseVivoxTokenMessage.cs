using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseVivoxTokenMessage : INetSerializable
    {
        public string token;

        public void Deserialize(NetDataReader reader)
        {
            token = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(token);
        }
    }
}