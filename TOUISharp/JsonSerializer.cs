using System;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing;
using System.Globalization;

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
            //MessageBox.Show(json);
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

                //Command.Add

                //decode to specific datatype
                var param = p.data;
	            var name = param.type.name.ToString();
	            var typedefinition = param.type.ToString();
	            var value = param.value.ToString();

                packet.Data.Type = DecodeTypeDefinition(name, typedefinition);
        		MessageBox.Show(packet.Data.Type.ToString() + ": " + value.ToString());
                packet.Data.Value = DecodeValue(packet.Data.Type, value);
        		//MessageBox.Show(packet.Data.Value.ToString());
        	}
            return packet;
        }

        private TypeDefinition DecodeTypeDefinition(string name, string typedefinition)
        {
            if (name == "boolean")
                return JsonConvert.DeserializeObject<TOUIBoolean>(typedefinition);
            else if (name == "number")
                return JsonConvert.DeserializeObject<TOUINumber>(typedefinition);
            else if (name == "vector2")
                return JsonConvert.DeserializeObject<TOUIVector2>(typedefinition);
            else if (name == "string")
                return JsonConvert.DeserializeObject<TOUIString>(typedefinition);
            else if (name == "color")
                return JsonConvert.DeserializeObject<TOUIColor>(typedefinition);
            else if (name == "enum")
                return JsonConvert.DeserializeObject<TOUIEnum>(typedefinition);
            else if (name == "array")
                return JsonConvert.DeserializeObject<TOUIArray>(typedefinition);
            else
                return null;
        }

        private object DecodeValue(TypeDefinition typedefinition, string value)
        {
            if (typedefinition is TOUIBoolean)
                return value;
            else if (typedefinition is TOUINumber)
        		return value;
            else if (typedefinition is TOUIVector2)
                return value;
            else if (typedefinition is TOUIString)
                return value;
            else if (typedefinition is TOUIColor)
        	{
        		return value;
//        		var comps = value.Split(',');
//        		return Color.FromArgb(0, int.Parse(comps[0]), int.Parse(comps[1]), int.Parse(comps[2]));
//        		return JsonConvert.DeserializeObject<System.Drawing.Color>(value + ", 0");
        	}                
            else if (typedefinition is TOUIEnum)
                return int.Parse(value);
            else if (typedefinition is TOUIArray)
                return JsonConvert.DeserializeObject(value);
            else 
                return null;
        }
    }
}
