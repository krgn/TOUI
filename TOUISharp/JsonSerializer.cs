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
                var serializerSettings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                json = JsonConvert.SerializeObject(packet/*, serializerSettings*/);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            //MessageBox.Show(json);
            return Encoding.ASCII.GetBytes(json);
        }
    	
    	static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public Packet Deserialize(byte[] bytes)
        {
            var json = Encoding.ASCII.GetString(bytes);

            //first decode as dynamic to check for the datatype
            dynamic p = JsonConvert.DeserializeObject(json);
            
        	var packet = new Packet();
            Command c;
            if (Command.TryParse(UppercaseFirst(p.command.ToString()), out c))
        	{
	            packet.Command = c;
	
	            if (packet.Command == Command.Init)
	                return packet;
	
	            packet.Data = new Parameter(p.data.id.ToString());
	
	            if (packet.Command == Command.Remove)
	                return packet;
	        	if (packet.Command == Command.Update)
	        	{
	        		packet.Data.Value = p.data.value;
	        		return packet;
	        	}
	
	            //decode to specific datatype
	            var vdn = p.data.type.name.ToString();
	            var vd = p.data.type.ToString();
	            var v = p.data.value.ToString();
	
	            DecodeValueDefition(packet, vdn, vd, v);
        	}
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
                packet.Data.Type = JsonConvert.DeserializeObject<TOUIVector2>(vd);
                packet.Data.Value = v.ToString();
            }
            else if (vdn.StartsWith("Boolean"))
            {
                packet.Data.Type = JsonConvert.DeserializeObject<TOUIBoolean>(vd);
                packet.Data.Value = bool.Parse(v);
            }
            else if (vdn.StartsWith("Number"))
            {
            	packet.Data.Type = JsonConvert.DeserializeObject<TOUINumber>(vd);
                packet.Data.Value = float.Parse(v);
            }
            else if (vdn.StartsWith("String"))
            {
                packet.Data.Type = JsonConvert.DeserializeObject<TOUIString>(vd);
                packet.Data.Value = v;
            }
            else if (vdn.StartsWith("Color"))
            {
                packet.Data.Type = JsonConvert.DeserializeObject<TOUIColor>(vd);
                packet.Data.Value = JsonConvert.DeserializeObject<System.Drawing.Color>(v + ", 0");
            }
            else if (vdn.StartsWith("Enum"))
            {
                packet.Data.Type = JsonConvert.DeserializeObject<TOUIEnum>(vd);
                packet.Data.Value = int.Parse(v);
            }
            else if (vdn.StartsWith("Dictionary"))
            {
                //MessageBox.Show(v);
            }
            //			var serializer = CsPickler.CreateJsonSerializer();
            //			return serializer.UnPickleOfString<Packet>(json);

            //return packet;
        }
    }
}
