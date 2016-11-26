using System;
using System.Linq;
using System.Collections.Generic;

using VVVV.Core.Logging;

namespace TOUI
{
    public class Server: Base 
	{
		private IServerTransporter FTransporter;
		public IServerTransporter Transporter
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
		
		public Action<Value> ValueUpdated;
		
		Dictionary<string, Value> FValues = new Dictionary<string, Value>();
		
		public string[] IDs 
		{
			get { return FValues.Keys.ToArray(); }
		}
		
		public override void Dispose()
		{
			if (FTransporter != null)
				FTransporter.Dispose();
		}
		
		public bool AddValue(Value value)
		{
			var result = false;
			if (!FValues.ContainsKey(value.Id))
			{
				FValues.Add(value.Id, value);
				result = true;
			}
			
			//dispatch to all clients via transporter
			var packet = Pack(Command.Add, value);
			Transporter.Send(packet);
			
			return result;
		}
		
		public bool UpdateValue(Value value)
		{
			var result = false;
			if (FValues.ContainsKey(value.Id))
				FValues.Remove(value.Id);
			
			FValues.Add(value.Id, value);
			result = true;
			
			//dispatch to all clients via transporter
			
			return result;
		}
		
		public bool RemoveValue(string id)
		{
			var result = FValues.Remove(id);
			
			//dispatch to all clients via transporter
			var packet = Pack(Command.Remove, id);
			Transporter.Send(packet);
			
			return result;
		}
		
		#region Transporter
		void ReceiveCB(Packet packet)
		{
			Logger.Log(LogType.Debug, "Server received packet from Client");
			
			switch (packet.Command)
			{
				case Command.Update:
				//inform the application
				if (ValueUpdated != null)
					ValueUpdated(packet.Data);
				break;
				
				case Command.Init:
				//client requests all values
				foreach (var value in FValues.Values)
					Transporter.Send(Pack(Command.Add, value));
				break;
			}
		}
		#endregion
	}
}