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
			//FTOUIServer.Transporter = new TOUI.WebsocketServerTransporter("127.0.0.1", 8181);
			//provide an ISerializer
			FTOUIServer.Serializer = new TOUI.JsonSerializer();
			
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
			//TODO: subscribe to subtype-pins here as well
			
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
			TypeDefinition typeDefinition = null;
			
			//figure out the actual spreadcount 
			//taking dimensions (ie. vectors) of value-spreads into account
			var subtype = pin.SubType.Split(',');
			var sliceCount = pin.SliceCount;
			object[] vs = new object[sliceCount];
			
			if (pin.Type == "Value")
			{
				/// values: guiType, dimension, default, min, max, stepSize, unitName, precision
				var dimensions = int.Parse(subtype[1]);
				//figure out the actual spreadcount 
				//taking dimensions (ie. vectors) of value-spreads into account
				sliceCount /= dimensions;

				vs = new object[sliceCount];
				
				if (dimensions == 1)
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
							var tbool = new TOUIBoolean();
							tbool.Default = dflt >= 0.5;
							if (subtype[0] == "Bang")
								tbool.Behavior = BoolBehavior.Bang;
							else if (subtype[0] == "Toggle")
								tbool.Behavior =BoolBehavior.Toggle;
							else if (subtype[0] == "Press")
								tbool.Behavior = BoolBehavior.Press;
							typeDefinition = tbool;
							
							for (int i=0; i<sliceCount; i++)
							{
								bool v;
								bool.TryParse(pin[i], out v);
								vs[i] = v;
							}
						}
						else
						{
							var number = new TOUINumber();
							number.Default = dflt;
							number.Min = min;
							number.Max = max;
							number.Step = intStep;
							number.Unit = subtype[6];
							typeDefinition = number;
							
							for (int i=0; i<sliceCount; i++)
							{
								int v;
								int.TryParse(pin[i], out v);
								vs[i] = v;
							}
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
						
						var number = new TOUINumber();
						number.Default = dflt;
						number.Min = min;
						number.Max = max;
						number.Step = floatStep;
						number.Unit = subtype[6].Trim();
						typeDefinition = number;
						
						for (int i=0; i<sliceCount; i++)
						{
							float v;
							float.TryParse(pin[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v);
							vs[i] = v;
						}
					}
				}
				else //dimensions > 1 
				{
					var itemCount = sliceCount*dimensions;
					var slices = new float[itemCount];
					for (int i=0; i<itemCount; i++)
						float.TryParse(pin[i], NumberStyles.Float, CultureInfo.InvariantCulture, out slices[i]);

					for (int i=0; i<sliceCount; i++)
						vs[i] = slices.Take(dimensions).ToArray();

					//vectorize the valueDefinition
					if (dimensions == 2)
					{
						typeDefinition = new TOUIVector2();
					} 
//					else if (dimensions == 3)
//					{
//						typeDefinition = new TOUIVector3();
//					}
//					else if (dimensions == 4)
//					{
//						typeDefinition = new TOUIVector4();
//					}
				}
			}
			else if (pin.Type == "String")
			{
				/// strings: guiType, default, fileMask, maxChars
				var tString = new TOUIString();
				tString.Default = subtype[1];
				typeDefinition = tString;
				
				for (int i=0; i<sliceCount; i++)
					vs[i] = pin[i];
			}
			else if (pin.Type == "Color")
			{
				/// colors: guiType, default, hasAlpha
				bool hasAlpha = subtype[2].Trim() == "HasAlpha";
				typeDefinition = new TOUIColor(hasAlpha);
				
				for (int i=0; i<sliceCount; i++)
				{
					var comps = pin[i].Split(',');
					var r = float.Parse(comps[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					var g = float.Parse(comps[1], NumberStyles.Float, CultureInfo.InvariantCulture);
					var b = float.Parse(comps[2], NumberStyles.Float, CultureInfo.InvariantCulture);
					var a = float.Parse(comps[3], NumberStyles.Float, CultureInfo.InvariantCulture);
					var rgba = new RGBAColor(r, g, b, a);
					vs[i] = rgba.Color;
				}
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
				var tEnum = new TOUIEnum(entries.ToArray());
				//tEnum.Default = dfault;
				typeDefinition = tEnum;
				
				for (int i=0; i<sliceCount; i++)
					vs[i] = entries.IndexOf(pin[i]);
			}
			
			if (typeDefinition == null)
			{
				typeDefinition = new TOUIString();
				parameter.Label = "Unknown Value";
			}
			else
				parameter.Label = pin.ParentNode.LabelPin.Spread.Trim('|');
        	
			//array
			if (sliceCount > 1)
			{
				parameter.Type = new TOUIArray(typeDefinition);
				parameter.Value = vs;
			}
			else
			{
				parameter.Type = typeDefinition;
				parameter.Value = vs[0];
			}
				
			var tag = pin.ParentNode.FindPin("Tag");
			if (tag != null)
				parameter.UserData = tag.Spread.Trim('|');
			
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