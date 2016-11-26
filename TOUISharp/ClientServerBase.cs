using System;
using VVVV.Core.Logging;

namespace TOUI
{
    public enum Command { Add, Update, Remove, Init };
    
    public abstract class Base: IDisposable
	{
		public ILogger Logger {get; set;}
		
		protected Packet Pack(Command command, Value value)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Data = value;
			
			return packet;
		}
		
		protected Packet Pack(Command command, string id)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Data = new Number(id, null, null);
			
			return packet;
		}
		
		protected Packet Pack(Command command)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Data = new Number("", null, null);
			
			return packet;
		}
		
		public virtual void Dispose()
		{
			Logger = null;
		}
	}
}