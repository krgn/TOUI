using System;
using System.Text;
using Newtonsoft.Json;

namespace TOUI
{
    public class JsonSerializer: ISerializer
	{
		public byte[] Serialize(Packet packet)
		{
			var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
			var json = JsonConvert.SerializeObject(packet/*, serializerSettings*/);
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
			
			//decode to specific datatype
			if (v.Data.Type == "Boolean")
				packet.Data = JsonConvert.DeserializeObject<TOUIBoolean>(v.Data.ToString());
			else if (v.Data.Type == "Number")
				packet.Data = JsonConvert.DeserializeObject<TOUINumber>(v.Data.ToString());
			else if (v.Data.Type == "String")
				packet.Data = JsonConvert.DeserializeObject<TOUIString>(v.Data.ToString());
			else if (v.Data.Type == "Color")
				packet.Data = JsonConvert.DeserializeObject<TOUIColor>(v.Data.ToString());
			else if (v.Data.Type == "Enum")
				packet.Data = JsonConvert.DeserializeObject<TOUIEnum>(v.Data.ToString());
			
			return packet;
		}
	}
}