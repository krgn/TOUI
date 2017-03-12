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
		
		public void Update(Parameter param)
		{
			Transporter.Send(Pack(Command.Update, param));
			Logger.Log(LogType.Debug, "Client sent Update");
		}
		
		public Action<Parameter> ParameterAdded;
		public Action<Parameter> ParameterUpdated;
		public Action<string> ParameterRemoved;
		
		void ReceiveCB(byte[] bytes)
		{
			var packet = Serializer.Deserialize(bytes);
			Logger.Log(LogType.Debug, "Client received: " + packet.Command.ToString());
			switch (packet.Command)
			{
				case Command.Add:
				//inform the application
				if (ParameterAdded != null)
					ParameterAdded(packet.Data);
				break;
				
				case Command.Update:
				//inform the application
				if (ParameterUpdated != null)
					ParameterUpdated(packet.Data);
				break;
				
				case Command.Remove:
				//inform the application
				if (ParameterRemoved != null)
					ParameterRemoved(packet.Data.ID);
				break;
			}
		}
	}
}