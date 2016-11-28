using System;
using System.Text;
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
			dynamic p = JsonConvert.DeserializeObject(json);
			
			var packet = new Packet();
			Command c;
			Command.TryParse(p.Command.ToString(), out c);
			packet.Command = c;
			
			if (packet.Command == Command.Init)
				return packet;
			
			packet.Parameter = new Parameter(p.Parameter.ID.ToString());
			
			//MessageBox.Show(packet.Command.ToString());
			
			if (packet.Command == Command.Remove)
				return packet;
			
			//decode to specific datatype
			var vdn = p.Parameter.ValueDefinition.Name.ToString();
			var vd = p.Parameter.ValueDefinition.ToString();
			var v = p.Parameter.Value.ToString();
			
//			MessageBox.Show(vd);
			
			DecodeValueDefition(packet, vdn, vd, v);
			return packet;
		}
		
		private void DecodeValueDefition(Packet packet, string vdn, string vd, string v)
		{
			if (vdn.StartsWith("Vector2"))
			{
				//expect something in the form: Vector2<Number<float32>,Number<float32>>
				//assume both components of the vector have the same type
				//so only look for the first one
				var dimensions = 0;
				
				//for assume float2
				//TODO: there must be a better way to deserialize this
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIVector2<TOUINumber<float>,TOUINumber<float>>>(vd);
				packet.Parameter.Value = v.ToString();
			}
			else if (vdn.StartsWith("Boolean"))
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIBoolean>(vd);
				packet.Parameter.Value = bool.Parse(v);
			}		
			else if (vdn.StartsWith("Number"))
			{	
				if (vdn.EndsWith("<int32>"))
				{
					packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUINumber<int>>(vd);
					packet.Parameter.Value = int.Parse(v);
				}
				else if (vdn.EndsWith("<float32>"))
				{
					packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUINumber<float>>(vd);
					packet.Parameter.Value = float.Parse(v);
				}
			}
			else if (vdn.StartsWith("String"))
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIString>(vd);
				packet.Parameter.Value = v;
			}
			else if (vdn.StartsWith("Color"))
			{	
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIColor>(vd);
				packet.Parameter.Value = JsonConvert.DeserializeObject<System.Drawing.Color>(v + ", 0");
			}
			else if (vdn.StartsWith("Enum"))
			{
				packet.Parameter.ValueDefinition = JsonConvert.DeserializeObject<TOUIEnum>(vd);
				packet.Parameter.Value = int.Parse(v);
			}
//			var serializer = CsPickler.CreateJsonSerializer();
//			return serializer.UnPickleOfString<Packet>(json);
			
			//return packet;
		}
	}
}