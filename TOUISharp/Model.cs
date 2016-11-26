using System;
using System.Drawing;
using Newtonsoft.Json;

namespace TOUI
{
    public class Packet
	{
		public Command Command { get; set; }
		public Value Data { get; set; }
	}
	
	public class Value
	{
		public string Id { get; set; }
		public string Type { get; set; }
		public object _Value { get; set; }
		public object Default { get; set; }
		public string Label { get; set; }
		public string Description { get; set; }
		//public Widget Widget { get; set; }
		public object UserData { get; set; }
		
		public Value(string id, object value)
		{
			Id = id;
			_Value = value;
		}
		
		public override string ToString()
		{
			var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
			return JsonConvert.SerializeObject(this);
		}
	}
	
	public class TOUIBoolean: Value
	{
		public TOUIBoolean(string id, object value)
		: base (id, value)
		{
			Type = "Boolean";
		}
	}
	
	public class TOUINumber: Value
	{
		public string Subtype { get; set; }
		public object Min { get; set; }
		public object Max { get; set; }
		public object Stepsize { get; set; }
		public string Unit { get; set; }
		public bool Cyclic { get; set; }
		public bool Pow2 { get; set; }
		
		public TOUINumber(string id, object value, string subtype)
		: base (id, value)
		{
			Type = "Number";
			Subtype = subtype;
		}
	}
	
	public class TOUIString: Value
	{
		public string Subtype { get; set; }
		public string Filemask { get; set; }
		public string MaxChars { get; set; }
		public bool Multiline { get; set; }
		
		public TOUIString(string id, string value, string subtype)
		: base (id, value)
		{
			Type = "String";
			Subtype = subtype;
		}
	}
	
	public class TOUIColor: Value
	{
		public string Subtype { get; set; }
		
		public TOUIColor(string id, Color value, string subtype)
		: base (id, value)
		{
			Type = "Color";
			Subtype = subtype;
		}
	}
	
	public class TOUIEnum: Value
	{
		public string[] Entries { get; set; }
		
		public TOUIEnum(string id, int value, string[] entries)
		: base (id, value)
		{
			Type = "Enum";
			Entries = entries;
		}
	}
}