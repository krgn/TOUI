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
		void Send(byte[] packet);
		Action<byte[]> Received {get; set;}
	}
	
	public interface IClientTransporter: IDisposable
	{
		void Send(byte[] packet);
		Action<byte[]> Received {get; set;}
	}
}