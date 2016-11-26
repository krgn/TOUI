#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Dynamic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using Newtonsoft.Json;

using TOUI;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "TOUI", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueTOUINode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Output("Output")]
		public ISpread<string> FOutput;
		
		[Output("Serialized")]
		public ISpread<string> FSerialized;

		[Import()]
		public ILogger FLogger;
		
		Server FTOUIServer;
		#endregion fields & pins

		public ValueTOUINode()
		{ 
			//initialize the TOUI Server
			FTOUIServer = new TOUI.Server();
			//provide an IServerTransporter
			FTOUIServer.Transporter = new TOUI.UDPServerTransporter("127.0.0.1", 4568, 4567); //implements 
			//provide an ISerializer
			FTOUIServer.Transporter.Serializer = new TOUI.JsonSerializer(); //implements 
			
			//subscribe to the value-updated event
			FTOUIServer.ValueUpdated = ValueUpdated;
		}
		
		public void Dispose()
		{
			FLogger.Log(LogType.Debug, "Disposing the TOUI Server");
			FTOUIServer.Dispose();
		}
		
		public void OnImportsSatisfied()
		{
			FTOUIServer.Logger = FLogger;
			
			//add/update/remove values
			FTOUIServer.AddValue(new Number("foo", 5, "int32") {Default = 0});
			FTOUIServer.AddValue(new Number("bar", 6, "int32") {Default = 1});
			FTOUIServer.AddValue(new Number("asds", 9.0, "float32") {Default = -2, Min = -3});
			FTOUIServer.AddValue(new _String("qwer", "hah", "string"));
			
//			FTOUIServer.RemoveValue("bar");
//			FTOUIServer.UpdateValue(id, ...);
		}
		
		private void ValueUpdated(Value value)
		{
			//a client has updated a value
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.AssignFrom(FTOUIServer.IDs);
			
//			var num = new _String("asds", "90", "string") {Default = ""};
//			var packet = new Packet();
//			packet.Command = Command.Init;
//			packet.Data = num;
//			var json = JsonConvert.SerializeObject(packet);
//			
//			dynamic v = JsonConvert.DeserializeObject(json);
//			
//			Command c = Command.Update;
//			Command.TryParse(v.Command.ToString(), out c);
//			packet.Command = c;
//			
//			if (v.Data.Type == "Number")
//				packet.Data = JsonConvert.DeserializeObject<Number>(v.Data.ToString());
//			else if (v.Data.Type == "String")
//				packet.Data = JsonConvert.DeserializeObject<_String>(v.Data.ToString());
//			
//			FSerialized[0] = packet.Command.ToString();
		}
	}
}
