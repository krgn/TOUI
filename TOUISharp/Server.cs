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
		
		public Action<Parameter> ParameterUpdated;
		
		Dictionary<string, Parameter> FParams = new Dictionary<string, Parameter>();
		
		public string[] IDs 
		{
			get { return FParams.Keys.ToArray(); }
		}
		
		public override void Dispose()
		{
			//remove all values?
			
			if (FTransporter != null)
				FTransporter.Dispose();
		}
		
		public bool AddParameter(Parameter value)
		{
			var result = false;
			if (!FParams.ContainsKey(value.ID))
			{
				FParams.Add(value.ID, value);
				result = true;
			}
			
			//dispatch to all clients via transporter
			var packet = Pack(Command.Add, value);
			Transporter.Send(packet);
			Logger.Log(LogType.Debug, "Server sent: Add");
			
			return result;
		}
		
		public bool UpdateParameter(Parameter value)
		{
			Logger.Log(LogType.Debug, "Server Update: " + value.ID);
			
			var result = false;
			if (FParams.ContainsKey(value.ID))
				FParams.Remove(value.ID);
			
			FParams.Add(value.ID, value);
			result = true;
			
			//dispatch to all clients via transporter
			var packet = Pack(Command.Update, value);
			
			
			Transporter.Send(packet);
			Logger.Log(LogType.Debug, "Server sent: Update");
			
			return result;
		}
		
		public bool RemoveParameter(string id)
		{
			var result = FParams.Remove(id);
			
			//dispatch to all clients via transporter
			var packet = Pack(Command.Remove, id);
			Transporter.Send(packet);
			Logger.Log(LogType.Debug, "Server sent: Remove");
			
			return result;
		}
		
		#region Transporter
		void ReceiveCB(byte[] bytes)
		{
			var packet = Serializer.Deserialize(bytes);
			Logger.Log(LogType.Debug, "Server received packet from Client: " + packet.Command.ToString());
			
			switch (packet.Command)
			{
				case Command.Update:
				//inform the application
				if (ParameterUpdated != null)
					ParameterUpdated(packet.Parameter);
				break;
				
				case Command.Init:
				//client requests all parameters
				foreach (var param in FParams.Values)
					Transporter.Send(Pack(Command.Add, param));
				break;
			}
		}
		#endregion
	}
}