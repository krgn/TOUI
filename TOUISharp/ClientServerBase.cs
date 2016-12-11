using System;
using VVVV.Core.Logging;

namespace TOUI
{
    public enum Command { Add, Update, Remove, Init };
    
    public abstract class Base: IDisposable
	{
		public ILogger Logger { get; set; }
		public ISerializer Serializer { get; set; }
		
		protected byte[] Pack(Command command, Parameter parameter)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Parameter = parameter;
			
			return Serializer.Serialize(packet);
		}
		
		protected byte[] Pack(Command command, string id)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Parameter = new Parameter(id);
			
			return Serializer.Serialize(packet);
		}
		
		protected byte[] Pack(Command command)
		{
			var packet = new Packet();
			packet.Command = command;
//			packet.Parameter = null; //new Parameter("");
			
			return Serializer.Serialize(packet);
		}
		
		public virtual void Dispose()
		{
			Logger = null;
		}
	}
}