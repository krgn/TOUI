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
			Logger.Log(LogType.Debug, "Client requests Init");
			Transporter.Send(Pack(Command.Init));
		}
		
		public Action<Value> ValueAdded;
		public Action<Value> ValueUpdated;
		public Action<string> ValueRemoved;
		
		void ReceiveCB(Packet packet)
		{
			switch (packet.Command)
			{
				case Command.Add:
				//inform the application
				if (ValueAdded != null)
					ValueAdded(packet.Data);
				break;
				
				case Command.Update:
				//inform the application
				if (ValueUpdated != null)
					ValueUpdated(packet.Data);
				break;
				
				case Command.Remove:
				//inform the application
				if (ValueRemoved != null)
					ValueRemoved(packet.Data.Id);
				break;
			}
		}
	}
}