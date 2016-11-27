#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using TOUI;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "TOUI", Category = "Value", Version = "Client", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ClientValueTOUINode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Init", IsBang=true)]
		public ISpread<bool> FInit;

		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		
		Dictionary<string, Parameter> FValues = new Dictionary<string, Parameter>();
		Client FTOUIClient;
		#endregion fields & pins
		
		public ClientValueTOUINode()
		{
			FTOUIClient = new Client();
			
			FTOUIClient.Transporter = new TOUI.UDPClientTransporter("127.0.0.1", 4567, 4568);
			FTOUIClient.Transporter.Serializer = new TOUI.JsonSerializer();
			
			FTOUIClient.ValueAdded = ValueAdded;
			FTOUIClient.ValueUpdated = ValueUpdated; 
			FTOUIClient.ValueRemoved = ValueRemoved;
		}
		
		public void OnImportsSatisfied()
		{
		  FTOUIClient.Logger = FLogger;
		}
		
		public void Dispose()
		{
			FLogger.Log(LogType.Debug, "Disposing the TOUI Client");
			FTOUIClient.Dispose();
		}
		
		private void ValueAdded(Parameter value)
		{
			if (!FValues.ContainsKey(value.ID))
				FValues.Add(value.ID, value);
		}
		
		private void ValueUpdated(Parameter value)
		{
			
		}
		
		private void ValueRemoved(string id)
		{
			FValues.Remove(id);
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//request all values from the server
			if (FInit[0])
			{
				FValues.Clear();
				FTOUIClient.Init();
			}
				
			FOutput.AssignFrom(FValues.Values.Select(v => 
			{
				if (v == null || v.ValueDefinition == null || v.Value == null)
				  return "null";
				else
				  return v.ID + ": " + v.ValueDefinition.ToString() + ": " + v.Value.ToString();
			}));
			//FOutput.AssignFrom(FValues.Values.Select(v => v.ID + ": " + v.ValueDefinition.ToString()));
		}
	}
}
