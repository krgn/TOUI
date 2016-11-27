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

using Newtonsoft.Json;

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
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Output("Output")]
		public ISpread<string> FOutput;
		
		[Output("Serialized")]
		public ISpread<string> FSerialized;

		[Import()]
		public ILogger FLogger;
		
		[Import()]
		public IHDEHost FHDEHost;
		
		Server FTOUIServer;
		Dictionary<string, IPin2> FCachedPins = new Dictionary<string, IPin2>();
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
			FTOUIServer.ValueUpdated = ValueUpdated;
		}
		
		public void OnImportsSatisfied()
		{
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;

			//get initial list of exposed ioboxes
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
				NodeAddedCB(node);
			
		  	FTOUIServer.Logger = FLogger;
		}
		
		public void Dispose()
		{
			//unscubscribe from nodeservice
			FHDEHost.ExposedNodeService.NodeAdded -= NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved -= NodeRemovedCB;
			
			//clear cached pins
			FCachedPins.Clear();
			
			//dispose the TOUI server
			FLogger.Log(LogType.Debug, "Disposing the TOUI Server");
			FTOUIServer.Dispose();
		}
		
		private void NodeAddedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			pin.Changed += PinChanged;
			
			var value = PinToValue(pin);
			FCachedPins.Add(value.Id, pin);
			
			FTOUIServer.AddValue(value);
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			pin.Changed -= PinChanged;
			
			var id = node.GetNodePath(false) + "/" + pinName;
			FCachedPins.Remove(id);
			
			FTOUIServer.RemoveValue(id);
		}
		
		private Value PinToValue(IPin2 pin)
		{
			var id = pin.ParentNode.GetNodePath(false) + "/" + pin.Name;
			
			Value value = null;
			
			var subtype = pin.SubType.Split(',');
			
			//figure out the actual spreadcount 
			//taking dimensions (ie. vectors) of value-spreads into account
			var sliceCount = pin.SliceCount;
			if (pin.Type == "Value")
			{
				var dimensions = 1;
				int.TryParse(subtype[1], out dimensions);
				sliceCount /= dimensions;
			}
			
			/// values: guiType, dimension, default, min, max, stepSize, unitName, precision
			
			FLogger.Log(LogType.Debug, pin.Type + ": " + sliceCount.ToString());
			if (pin.Type == "Value")
			{
				{
					var dimensions = subtype[1].Trim();
					if (dimensions == "1")
					{
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
								value = new TOUIBoolean(id, pin[0] == "1");
								value.Default = dflt;
							}
							else
							{
								int v;
								int.TryParse(pin[0], out v);
								var number = new TOUINumber(id, v, "int32");
								number.Default = dflt;
								number.Min = min;
								number.Max = max;
								number.Stepsize = intStep;
								number.Unit = subtype[6];
								value = number;
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
							
							object v;
							float f = 0;
							if (sliceCount == 1)
							{
								float.TryParse(pin[0], NumberStyles.Float, CultureInfo.InvariantCulture, out f);
								v = f;
							}
							else
							{
								float[] fs = new float[sliceCount];
								for (int i = 0; i<sliceCount; i++)
								{
									f = 0;
									float.TryParse(pin[i], NumberStyles.Float, CultureInfo.InvariantCulture, out f);
									fs[i] = f;
								}	
								v = fs;								
							}
							
							var number = new TOUINumber(id, v, "float32");
							number.Default = dflt;
							number.Min = min;
							number.Max = max;
							number.Stepsize = floatStep;
							number.Unit = subtype[6].Trim();
							value = number;
						}
					}
					//else //vectors
				}
			}
			else if (pin.Type == "String")
			{
				/// strings: guiType, default, fileMask, maxChars
				var s = new TOUIString(id, pin[0], "");
				s.Default = subtype[1];
				value = s;
			}
			else if (pin.Type == "Color")
			{
				/// colors: guiType, default, hasAlpha
				var comps = pin[0].Split(',');
				var rgba = new RGBAColor(float.Parse(comps[0]), float.Parse(comps[1]), float.Parse(comps[2]), float.Parse(comps[3]));
				
				bool hasAlpha = subtype[2].Trim() == "HasAlpha";
				value = new TOUIColor(id, rgba.Color, "RGB" + (hasAlpha ? "A" : ""));
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
				value = new TOUIEnum(id, val, entries.ToArray()) { Default = dfault } ;
			}
			
			if (value == null)
			{
				value = new TOUIString(id, "", "");
				value.Label = "Unknown Value";
			}
			
			value.Label = pin.ParentNode.LabelPin.Spread.Trim('|');
			
			var tag = pin.ParentNode.FindPin("Tag");
			if (tag != null)
				value.UserData = tag.Spread.Trim('|');
        	
			return value;
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
			
			FTOUIServer.UpdateValue(PinToValue(pin));
		}
		
		//a TOUI client has updated a value
		private void ValueUpdated(Value value)
		{
			IPin2 pin;
			if (FCachedPins.TryGetValue(value.Id, out pin))
			{
				pin.Spread = value._Value.ToString();
			}
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.AssignFrom(FTOUIServer.IDs);
		}
	}
}
