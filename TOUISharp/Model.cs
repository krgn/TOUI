using System;
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
		public object Userdata { get; set; }
		
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
	
	public class Number: Value
	{
		public string Subtype { get; set; }
		public object Min { get; set; }
		public object Max { get; set; }
		public object Stepsize { get; set; }
		public string Unit { get; set; }
		public bool Cyclic { get; set; }
		public bool Pow2 { get; set; }
		
		public Number(string id, object value, string subtype)
		: base (id, value)
		{
			Type = "Number";
			Subtype = subtype;
		}

		public override string ToString()
		{
			return base.ToString() + " number";
		}
	}
	
	public class _String: Value
	{
		public string Subtype { get; set; }
		public string Filemask { get; set; }
		public string MaxChars { get; set; }
		public bool Multiline { get; set; }
		
		public _String(string id, string value, string subtype)
		: base (id, value)
		{
			Type = "String";
			Subtype = subtype;
		}

		public override string ToString()
		{
			return base.ToString() + " string";
		}
	}
}