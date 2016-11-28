#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Globalization;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

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
				Help = "A TOUI Server", 
				Tags = "remote")]
	#endregion PluginInfo
	public class ValueTOUINode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<int> FInput;

		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		
		[Import()]
		public IHDEHost FHDEHost;
		
		Server FTOUIServer;
		Dictionary<string, IPin2> FCachedPins = new Dictionary<string, IPin2>();
		List<Parameter> FMessageQueue = new List<Parameter>();
		#endregion fields & pins

		public ValueTOUINode()
		{
			//initialize the TOUI Server
			FTOUIServer = new TOUI.Server();
			//provide an IServerTransporter
			FTOUIServer.Transporter = new TOUI.UDPServerTransporter("127.0.0.1", 4568, 4567);
			//provide an ISerializer
			FTOUIServer.Transporter.Serializer = new TOUI.JsonSerializer();
			
			//subscribe to the value-updated event
			FTOUIServer.ParameterUpdated = ParameterUpdated;
		}
		
		public void OnImportsSatisfied()
		{
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;

			FTOUIServer.Logger = FLogger;
			
			//get initial list of exposed ioboxes
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
				NodeAddedCB(node);
			
			//a dictionary test
//			var keyDef = new TOUIString();
//			var valueDef = new TOUINumber<float>();
//			var dictDef = new TOUIDictionary<TOUIString, TOUINumber<float>>(keyDef, valueDef);
			
//			var param = new Parameter("fooID");
//			param.ValueDefinition = dictDef;
//			var dict = new Dictionary<string, float>();
//			dict.Add("entry1", 0.0012f);
//			dict.Add("entry3", 0.3f);
//			dict.Add("entry5", 5.9f);
//			param.Value = dict;
//			FTOUIServer.AddParameter(param);
		}
		
		public void Dispose() 
		{ 
			//unscubscribe from nodeservice
			FHDEHost.ExposedNodeService.NodeAdded -= NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved -= NodeRemovedCB;
			//dispose the TOUI server
			FLogger.Log(LogType.Debug, "Disposing the TOUI Server");
			FTOUIServer.Dispose();
			
			//clear cached pins
			FCachedPins.Clear();
			
			FMessageQueue.Clear();
		}
		
		private void NodeAddedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			pin.Changed += PinChanged;
			
			var value = PinToParameter(pin);
			FCachedPins.Add(value.ID, pin);
			
			FTOUIServer.AddParameter(value);
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			pin.Changed -= PinChanged;
			
			var id = node.GetNodePath(false) + "/" + pinName;
			FCachedPins.Remove(id);
			
			FTOUIServer.RemoveParameter(id);
		}
		
		private Parameter PinToParameter(IPin2 pin)
		{
			var id = pin.ParentNode.GetNodePath(false) + "/" + pin.Name;
			
			Parameter parameter = new Parameter(id);
			ValueDefinition valueDefinition = null;
			object value = null;
			
			//figure out the actual spreadcount 
			//taking dimensions (ie. vectors) of value-spreads into account
			var subtype = pin.SubType.Split(',');
			var sliceCount = pin.SliceCount;
			if (pin.Type == "Value")
			{
				var dimensions = 1;
				int.TryParse(subtype[1], out dimensions);
				sliceCount /= dimensions;
			}
			
			FLogger.Log(LogType.Debug, pin.Type + ": " + sliceCount.ToString());
			
			if (pin.Type == "Value")
			{
				/// values: guiType, dimension, default, min, max, stepSize, unitName, precision
				int intStep = 0;
				float floatStep = 0;
				if (int.TryParse(subtype[5], out intStep)) //integer
				{
					int dflt = 0;
					int.TryParse(subtype[2], out dflt);
					int min = 0;
					int.TryParse(subtype[3], out min);
					int max = 0;
					int.TryParse(subtype[4], out max);
					
					var isbool = (min == 0) && (max == 1);
					if (isbool)
					{
						valueDefinition = new TOUIBoolean();
						valueDefinition.Default = dflt;
					}
					else
					{
						var number = new TOUINumber<int>();
						number.Default = dflt;
						number.Min = min;
						number.Max = max;
						number.Stepsize = intStep;
						number.Unit = subtype[6];
						valueDefinition = number;
					}
				}
				else if (float.TryParse(subtype[5], NumberStyles.Float, CultureInfo.InvariantCulture, out floatStep))
				{
					float dflt = 0;
					float.TryParse(subtype[2], NumberStyles.Float, CultureInfo.InvariantCulture, out dflt);
					float min = 0;
					float.TryParse(subtype[3], NumberStyles.Float, CultureInfo.InvariantCulture, out min);
					float max = 0;
					float.TryParse(subtype[4], NumberStyles.Float, CultureInfo.InvariantCulture, out max);
					float precision = 0;
					float.TryParse(subtype[7], NumberStyles.Float, CultureInfo.InvariantCulture, out precision);
					
					var number = new TOUINumber<float>();
					number.Default = dflt;
					number.Min = min;
					number.Max = max;
					number.Stepsize = floatStep;
					number.Unit = subtype[6].Trim();
					valueDefinition = number;
				}
				
				var dimensions = int.Parse(subtype[1]);
				if (valueDefinition is TOUIBoolean)
				{
					value = pin[0] == "1";
				}
				else if (valueDefinition is TOUINumber<int>)
				{
					int[] vs = new int[sliceCount];
					for (int i=0; i<sliceCount; i++)
						int.TryParse(pin[i], out vs[i]);
					
					if (sliceCount == 1)
						value = vs[0];
					else
						value = vs;
				}
				else if (valueDefinition is TOUINumber<float>)
				{
					var itemCount = sliceCount*dimensions;
					float[] vs = new float[itemCount];
					for (int i=0; i<itemCount; i++)
						float.TryParse(pin[i], NumberStyles.Float, CultureInfo.InvariantCulture, out vs[i]);
					
					if (itemCount == 1)
						value = vs[0];
					else
					{
						object[] slices = new object[sliceCount];
						for (int i=0; i<sliceCount; i++)
							slices[i] = vs.Take(dimensions).ToArray();
						
						value = slices;
					}

					//vectorize the valueDefinition
					if (dimensions == 2)
					{
						var vectorXDefinition = (TOUINumber<float>) valueDefinition;
						var vectorYDefinition = (TOUINumber<float>) valueDefinition;
						valueDefinition = new TOUIVector2<TOUINumber<float>, TOUINumber<float>>(vectorXDefinition, vectorYDefinition);
					} 
					else if (dimensions == 3)
					{
						var vectorXDefinition = (TOUINumber<float>) valueDefinition;
						var vectorYDefinition = (TOUINumber<float>) valueDefinition;
						var vectorZDefinition = (TOUINumber<float>) valueDefinition;
						valueDefinition = new TOUIVector3<TOUINumber<float>, TOUINumber<float>, TOUINumber<float>>(vectorXDefinition, vectorYDefinition, vectorZDefinition);
					}
					else if (dimensions == 4)
					{
						var vectorXDefinition = (TOUINumber<float>) valueDefinition;
						var vectorYDefinition = (TOUINumber<float>) valueDefinition;
						var vectorZDefinition = (TOUINumber<float>) valueDefinition;
						var vectorWDefinition = (TOUINumber<float>) valueDefinition;
						valueDefinition = new TOUIVector4<TOUINumber<float>, TOUINumber<float>, TOUINumber<float>, TOUINumber<float>>(vectorXDefinition, vectorYDefinition, vectorZDefinition, vectorWDefinition);
					}
				}
			}
			else if (pin.Type == "String")
			{
				/// strings: guiType, default, fileMask, maxChars
				valueDefinition = new TOUIString();
				valueDefinition.Default = subtype[1];
				value = pin[0];
			}
			else if (pin.Type == "Color")
			{
				/// colors: guiType, default, hasAlpha
				var comps = pin[0].Split(',');
				var rgba = new RGBAColor(float.Parse(comps[0]), float.Parse(comps[1]), float.Parse(comps[2]), float.Parse(comps[3]));
				
				bool hasAlpha = subtype[2].Trim() == "HasAlpha";
				valueDefinition = new TOUIColor("RGB" + (hasAlpha ? "A" : ""));
				
				value = rgba.Color;
			}
			else if (pin.Type == "Enumeration")
			{
				/// enums: guiType, enumName, default
				var enumName = subtype[1].Trim();
				var entryCount = EnumManager.GetEnumEntryCount(enumName);
				var entries = new List<string>();
				for (int i = 0; i < entryCount; i++)
					entries.Add(EnumManager.GetEnumEntryString(enumName, i));
					
				var dfault = entries.IndexOf(subtype[2].Trim());
				var val = entries.IndexOf(pin[0]); 
				valueDefinition = new TOUIEnum(entries.ToArray());
				valueDefinition.Default = dfault;
				value = entries.IndexOf(pin[0]);
			}
			
			if (valueDefinition == null)
			{
				valueDefinition = new TOUIString();
				valueDefinition.Label = "Unknown Value";
			}
			else
				valueDefinition.Label = pin.ParentNode.LabelPin.Spread.Trim('|');
			
			var tag = pin.ParentNode.FindPin("Tag");
			if (tag != null)
				valueDefinition.UserData = tag.Spread.Trim('|');
        	
			parameter.ValueDefinition = valueDefinition;
			parameter.Value = value;
			
			//FLogger.Log(LogType.Debug, value.ToString());
			
			return parameter;
		}
		
		private string PinNameFromNode(INode2 node)
		{
			string pinName = "";
			if (node.NodeInfo.Systemname == "IOBox (Value Advanced)")
				pinName = "Y Input Value";
			else if (node.NodeInfo.Systemname == "IOBox (String)")
				pinName = "Input String";
			else if (node.NodeInfo.Systemname == "IOBox (Color)")
				pinName = "Color Input";
			else if (node.NodeInfo.Systemname == "IOBox (Enumerations)")
				pinName = "Input Enum";
			else if (node.NodeInfo.Systemname == "IOBox (Node)")
				pinName = "Input Node";
			
			return pinName;
		}
		
		//the application updated a value
		private void PinChanged(object sender, EventArgs e)
		{
			var pin = sender as IPin2;
			FTOUIServer.UpdateParameter(PinToParameter(pin));
		}
		
		//a TOUI client has updated a value
		private void ParameterUpdated(Parameter value)
		{
			lock(FMessageQueue)
				FMessageQueue.Add(value);
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//process messagequeue
			//in order to handle all messages from main thread
			//since all COM-access is single threaded
			lock(FMessageQueue)
			{
				foreach (var param in FMessageQueue)
				{
					IPin2 pin;
					if (FCachedPins.TryGetValue(param.ID, out pin))
					{
						if (param.Value is float)
							pin.Spread = ((float)param.Value).ToString(System.Globalization.CultureInfo.InvariantCulture);
						else 
							pin.Spread = param.Value.ToString();
					}
				}
				FMessageQueue.Clear();
			}
			
			FOutput.AssignFrom(FCachedPins.Keys);
		}
	}
}
