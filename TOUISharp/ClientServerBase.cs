using System;
using VVVV.Core.Logging;

namespace TOUI
{
    public enum Command { Add, Update, Remove, Init };
    
    public abstract class Base: IDisposable
	{
		public ILogger Logger {get; set;}
		
		protected Packet Pack(Command command, Parameter parameter)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Parameter = parameter;
			
			return packet;
		}
		
		protected Packet Pack(Command command, string id)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Parameter = new Parameter(id);
			
			return packet;
		}
		
		protected Packet Pack(Command command)
		{
			var packet = new Packet();
			packet.Command = command;
//			packet.Parameter = null; //new Parameter("");
			
			return packet;
		}
		
		public virtual void Dispose()
		{
			Logger = null;
		}
	}
}