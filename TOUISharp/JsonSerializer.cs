using System;
using System.Text;
using Nessos.CsPickler;
using Nessos.FsPickler;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TOUI
{
    public class JsonSerializer: ISerializer
	{
		public byte[] Serialize(Packet packet)
		{
			var json = "";
			try
			{
//			var serializer = CsPickler.CreateJsonSerializer();
//			json = serializer.PickleToString(packet);			
				var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
				json = JsonConvert.SerializeObject(packet/*, serializerSettings*/);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
			return Encoding.ASCII.GetBytes(json);	
		}
		
		public Packet Deserialize(byte[] bytes)
		{
			var json = Encoding.ASCII.GetString(bytes);
			
			//first decode as dynamic to check for the datatype
			dynamic v = JsonConvert.DeserializeObject(json);
			
			var packet = new Packet();
			Command c;
			Command.TryParse(v.Command.ToString(), out c);
			packet.Command = c;
			
			if (packet.Command == Command.Init)
				return packet;
			
			packet.Parameter = new Parameter(v.Parameter.ID.ToString());
			
			if (packet.Command == Command.Remove)
				return packet;
			
			//decode to specific datatype
			var vdn = v.Parameter.ValueDefinition.Name.ToString();
			if (vdn == "Boolean")
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIBoolean>(v.Parameter.ValueDefinition.ToString());
				packet.Parameter.Value = bool.Parse(v.Parameter.Value.ToString());
			}		
			else if (vdn.StartsWith("Number"))
			{	
				if (vdn.EndsWith("<int32>"))
				{
					packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUINumber<int>>(v.Parameter.ValueDefinition.ToString());
					packet.Parameter.Value = int.Parse(v.Parameter.Value.ToString());
				}
				else if (vdn.EndsWith("<float32>"))
				{
					packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUINumber<float>>(v.Parameter.ValueDefinition.ToString());
					packet.Parameter.Value = float.Parse(v.Parameter.Value.ToString());
				}
			}
			else if (vdn == "String")
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIString>(v.Parameter.ValueDefinition.ToString());
				packet.Parameter.Value = v.Parameter.Value.ToString();
			}
			else if (vdn == "Color")
			{	
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIColor>(v.Parameter.ValueDefinition.ToString());
				packet.Parameter.Value = JsonConvert.DeserializeObject<System.Drawing.Color>(v.Parameter.Value.ToString());
			}
			else if (vdn == "Enum")
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIEnum>(v.Parameter.ValueDefinition.ToString());
				packet.Parameter.Value = JsonConvert.DeserializeObject<int>(v.Parameter.Value.ToString());
			}
//			var serializer = CsPickler.CreateJsonSerializer();
//			return serializer.UnPickleOfString<Packet>(json);
			
			return packet;
		}
	}
}