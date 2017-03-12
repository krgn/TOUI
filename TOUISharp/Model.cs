using System;
using System.Drawing;
using Newtonsoft.Json;

namespace TOUI
{
    public class Packet
	{
        [JsonProperty("command")]
        public Command Command { get; set; }
        [JsonProperty("parameter")]
        public Parameter Parameter { get; set; }
	}
	
	public class Parameter
	{
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("valuedefinition")]
        public ValueDefinition ValueDefinition { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }
		//public Widget Widget { get; set; }
		
		public Parameter (string id)
		{
			ID = id;
//			ValueDefinition = null;
//			Value = null;
		}
	}
	
	public class ValueDefinition
	{
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("default")]
        public object Default { get; set; }
        [JsonProperty("userdata")]
        public object UserData { get; set; }
		
		public ValueDefinition(string name, object _default)
		{
			Name = name;
			Default = _default;
		}
		
		public override string ToString()
		{
		//	var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
			return JsonConvert.SerializeObject(this);
		}
	}
	
	public class TOUIBoolean: ValueDefinition
	{
		public TOUIBoolean()
		: base ("Boolean", false)
		{
		}
	}
	
	public class TOUINumber<T>: ValueDefinition
	{
        [JsonProperty("min")]
        public T Min { get; set; }
        [JsonProperty("max")]
        public T Max { get; set; }
        [JsonProperty("stepsize")]
        public T Stepsize { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
        [JsonProperty("cyclic")]
        public bool Cyclic { get; set; }
        [JsonProperty("pow2")]
        public bool Pow2 { get; set; }
		
		public TOUINumber()
		: base ("Number", 0)
		{
			if (typeof(T).ToString() == "System.Int32")
				Name += "<int32>";
			else if (typeof(T).ToString() == "System.Single")
				Name += "<float32>";
		}
	}
	
	public class TOUIString: ValueDefinition
	{
        [JsonProperty("subtype")]
        public string Subtype { get; set; }
        [JsonProperty("filemask")]
        public string Filemask { get; set; }
        [JsonProperty("maxchars")]
        public string MaxChars { get; set; }
        [JsonProperty("multiline")]
        public bool Multiline { get; set; }
		
		public TOUIString()
		: base ("String", "")
		{
		}
	}
	
	public class TOUIColor: ValueDefinition
	{
        [JsonProperty("subtype")]
        public string Subtype { get; set; }
		
		public TOUIColor(string subtype)
		: base ("Color", null)
		{
			Subtype = subtype;
		}
	}
	
	public class TOUIEnum: ValueDefinition
	{
        [JsonProperty("entries")]
        public string[] Entries { get; set; }
		
		public TOUIEnum(string[] entries)
		: base ("Enum", 0)
		{
			Entries = entries;
		}
	}
	
	public class TOUIVector2<X, Y>: ValueDefinition
	where X: ValueDefinition
	where Y: ValueDefinition
	{
        [JsonProperty("xsubtype")]
		public X XSubtype { get; set; }
        [JsonProperty("ysubtype")]
		public Y YSubtype { get; set; }
		
		public TOUIVector2(X xSubtype, Y ySubtype)
		: base ("Vector2<" + xSubtype.Name + "," + ySubtype.Name + ">", null)
		{
			XSubtype = xSubtype;
			YSubtype = ySubtype;
		}
	}
	
	public class TOUIVector3<X, Y, Z>: ValueDefinition
	where X: ValueDefinition
	where Y: ValueDefinition
	where Z: ValueDefinition
	{
        [JsonProperty("xsubtype")]
		public X XSubtype { get; set; }
        [JsonProperty("ysubtype")]
        public Y YSubtype { get; set; }
        [JsonProperty("zsubtype")]
        public Z ZSubtype { get; set; }
		
		public TOUIVector3(X xSubtype, Y ySubtype, Z zSubtype)
		: base ("Vector3<" + xSubtype.Name + "," + ySubtype.Name + "," + zSubtype.Name + ">", null)
		{
			XSubtype = xSubtype;
			YSubtype = ySubtype;
			ZSubtype = zSubtype;
		}
	}
	
	public class TOUIVector4<X, Y, Z, W>: ValueDefinition
	where X: ValueDefinition
	where Y: ValueDefinition
	where Z: ValueDefinition
	where W: ValueDefinition
	{
        [JsonProperty("xsubtype")]
        public X XSubtype { get; set; }
        [JsonProperty("ysubtype")]
        public Y YSubtype { get; set; }
        [JsonProperty("zsubtype")]
        public Z ZSubtype { get; set; }
        [JsonProperty("wsubtype")]
        public W WSubtype { get; set; }
		
		public TOUIVector4(X xSubtype, Y ySubtype, Z zSubtype, W wSubtype)
		: base ("Vector4<" + xSubtype.Name + "," + ySubtype.Name + "," + zSubtype.Name + "," + wSubtype.Name + ">", null)
		{
			XSubtype = xSubtype;
			YSubtype = ySubtype;
			ZSubtype = zSubtype;
			WSubtype = wSubtype;
		}
	}
	
	public class TOUIDictionary<K, V>: ValueDefinition
	where K: ValueDefinition
	where V: ValueDefinition
	{
        [JsonProperty("key")]
        public K KeyDefinition { get; set; }
        [JsonProperty("value")]
        public V ValueDefinition { get; set; }
		
		public TOUIDictionary(K keyDefinition, V valueDefinition)
		: base ("Dictionary", null)
		{
			KeyDefinition = keyDefinition;
			ValueDefinition = valueDefinition;
		}
	}
}