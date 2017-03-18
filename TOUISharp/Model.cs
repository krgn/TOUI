using System;
using System.Drawing;
using Newtonsoft.Json;

namespace TOUI
{
    public class Packet
	{
        [JsonProperty("command")]
        public Command Command { get; set; }
        [JsonProperty("data")]
        public Parameter Data { get; set; }
	}
	
	public class Parameter
	{
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("type")]
        public TypeDefinition Type { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("widget")]
        public string Widget { get; set; }
        [JsonProperty("default")]
        public object Default { get; set; }
        [JsonProperty("userdata")]
        public object UserData { get; set; }

        public Parameter (string id)
		{
			ID = id;
//			ValueDefinition = null;
//			Value = null;
		}
	}
	
	public class TypeDefinition
	{
        [JsonProperty("name")]
        public string Name { get; set; }
		
		public TypeDefinition(string name)
		{
			Name = name;
		}
		
		public override string ToString()
		{
		//	var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
			return JsonConvert.SerializeObject(this);
		}
	}
	
    public enum BoolBehavior { Toggle, Bang, Press }
	public class TOUIBoolean: TypeDefinition
	{
        [JsonProperty("default")]
        public bool Default { get; set; }
        [JsonProperty("behavior")]
        public BoolBehavior Behavior { get; set; }

        public TOUIBoolean()
		: base ("boolean")
		{
		}
	}
	
	public class TOUINumber: TypeDefinition
	{
        [JsonProperty("precision")]
        public int Precision { get; set; }
        [JsonProperty("min")]
        public float Min { get; set; }
        [JsonProperty("max")]
        public float Max { get; set; }
        [JsonProperty("step")]
        public float Step { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
        [JsonProperty("cyclic")]
        public bool Cyclic { get; set; }
        [JsonProperty("pow2")]
        public bool Pow2 { get; set; }
        [JsonProperty("default")]
        public float Default { get; set; }

        public TOUINumber()
		: base ("number")
		{
		}
	}
	
	public class TOUIVector2: TypeDefinition
	{
        [JsonProperty("precision")]
        public int Precision { get; set; }
        [JsonProperty("min")]
        public PointF Min { get; set; }
        [JsonProperty("max")]
        public PointF Max { get; set; }
        [JsonProperty("step")]
        public float Step { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
        [JsonProperty("cyclic")]
        public bool Cyclic { get; set; }
        [JsonProperty("pow2")]
        public bool Pow2 { get; set; }
        [JsonProperty("default")]
        public PointF Default { get; set; }

        public TOUIVector2()
		: base ("vector2")
		{
		}
	}
	
	public class TOUIString : TypeDefinition
    {
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("filemask")]
        public string Filemask { get; set; }
        [JsonProperty("maxchars")]
        public string MaxChars { get; set; }
        [JsonProperty("default")]
        public string Default { get; set; }

        public TOUIString()
        : base("string")
        {
        }
    }

    public class TOUIColor : TypeDefinition
    {
        [JsonProperty("alpha")]
        public bool Alpha { get; set; }
        [JsonProperty("default")]
        public Color Default { get; set; }

        public TOUIColor(bool hasAlpha)
        : base("color")
        {
        	Alpha = hasAlpha;
        }
    }

    public class TOUIEnum : TypeDefinition
    {
        [JsonProperty("entries")]
        public string[] Entries { get; set; }

        public TOUIEnum(string[] entries)
        : base("enum")
        {
            Entries = entries;
        }
    }

    public class TOUIArray : TypeDefinition
    {
        [JsonProperty("subtype")]
        public TypeDefinition Subtype { get; set; }

        public TOUIArray(TypeDefinition subtype)
        : base("array")
        {
        	Subtype = subtype;
        }
    }

    //   public class TOUIDictionary<K, V>: TypeDefinition
    //where K: TypeDefinition
    //where V: TypeDefinition
    //{
    //       [JsonProperty("key")]
    //       public K KeyDefinition { get; set; }
    //       [JsonProperty("value")]
    //       public V ValueDefinition { get; set; }

    //	public TOUIDictionary(K keyDefinition, V valueDefinition)
    //	: base ("Dictionary", null)
    //	{
    //		KeyDefinition = keyDefinition;
    //		ValueDefinition = valueDefinition;
    //	}
    //}
}