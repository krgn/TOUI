using System;

namespace TOUI
{
    public interface ISerializer 
	{
		byte[] Serialize(Packet packet);
		Packet Deserialize(byte[] bytes);
	}
	
	public interface IServerTransporter: IDisposable
	{
		ISerializer Serializer {get; set;}
		void Send(Packet packet);
		Action<Packet> Received {get; set;}
	}
	
	public interface IClientTransporter: IDisposable
	{
		ISerializer Serializer { get; set; }
		void Send(Packet packet);
		Action<Packet> Received {get; set;}
	}
}