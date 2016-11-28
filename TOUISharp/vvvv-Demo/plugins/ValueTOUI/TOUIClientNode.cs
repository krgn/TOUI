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
	[PluginInfo(Name = "TOUI", 
				Category = "VVVV", 
				Version = "Client", 
				Help = "A simple TOUI client test", Tags = "")]
	#endregion PluginInfo
	public class ClientValueTOUINode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Init", IsBang=true)]
		public ISpread<bool> FInit;

		[Input("Input")]
		public IDiffSpread<int> FInput;
		
		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		
		Dictionary<string, Parameter> FParams = new Dictionary<string, Parameter>();
		Client FTOUIClient;
		#endregion fields & pins
		
		public ClientValueTOUINode()
		{
			FTOUIClient = new Client();
			
			FTOUIClient.Transporter = new TOUI.UDPClientTransporter("127.0.0.1", 4567, 4568);
			FTOUIClient.Transporter.Serializer = new TOUI.JsonSerializer();
			
			FTOUIClient.ParameterAdded = ParameterAdded;
			FTOUIClient.ParameterUpdated = ParameterUpdated; 
			FTOUIClient.ParameterRemoved = ParameterRemoved;
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
		
		private void ParameterAdded(Parameter param)
		{
			if (!FParams.ContainsKey(param.ID))
				FParams.Add(param.ID, param);
		}
		
		private void ParameterUpdated(Parameter param)
		{
			if (FParams.ContainsKey(param.ID))
				FParams[param.ID].Value = param.Value;
		}
		
		private void ParameterRemoved(string id)
		{
			FParams.Remove(id);
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//request all values from the server
			if (FInit[0])
			{
				FParams.Clear();
				FTOUIClient.Init();
			}
			
			//feedback value test
			if (FInput.IsChanged)
			{
				var param = FParams.Values.Where(p => p.ValueDefinition.Label == "My Float").FirstOrDefault();
				if (param != null)
				{
					param.Value = FInput[0];
					FTOUIClient.Update(param);
				}
			}
				
			FOutput.AssignFrom(FParams.Values.Select(v => 
			{
				if (v.ValueDefinition == null || v.Value == null)
				  return v.ID;
				else
				  return v.ID + ": " + v.ValueDefinition.ToString() + ": " + v.Value.ToString();
			}));
		}
	}
}
