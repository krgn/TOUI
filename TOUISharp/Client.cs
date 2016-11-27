using System;
using VVVV.Core.Logging;

namespace TOUI
{
    public class Client: Base
	{
		private IClientTransporter FTransporter;
		public IClientTransporter Transporter
		{ 
			get { return FTransporter; }
			
			set 
			{
				if (FTransporter != null)
					FTransporter.Dispose();
				
				FTransporter = value;	
				FTransporter.Received = ReceiveCB;
			}
		}
		
		public override void Dispose()
		{
			if (FTransporter != null)
				FTransporter.Dispose();
		}
		
		public void Init()
		{
			Transporter.Send(Pack(Command.Init));
			Logger.Log(LogType.Debug, "Client sent Init");
		}
		
		public Action<Parameter> ValueAdded;
		public Action<Parameter> ValueUpdated;
		public Action<string> ValueRemoved;
		
		void ReceiveCB(Packet packet)
		{
			Logger.Log(LogType.Debug, "Client received: " + packet.Command.ToString());
			switch (packet.Command)
			{
				case Command.Add:
				//inform the application
				if (ValueAdded != null)
					ValueAdded(packet.Parameter);
				break;
				
				case Command.Update:
				//inform the application
				if (ValueUpdated != null)
					ValueUpdated(packet.Parameter);
				break;
				
				case Command.Remove:
				//inform the application
				if (ValueRemoved != null)
					ValueRemoved(packet.Parameter.ID);
				break;
			}
		}
	}
}