using System;
using System.Drawing;
using Newtonsoft.Json;

namespace TOUI
{
    public class Packet
	{
		public Command Command { get; set; }
		public Parameter Parameter { get; set; }
	}
	
	public class Parameter
	{
		public string ID { get; set; }
		public ValueDefinition ValueDefinition { get; set; }
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
		public string Name { get; set; }
		public string Label { get; set; }
		public string Description { get; set; }
		public object Default { get; set; }
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
		public T Min { get; set; }
		public T Max { get; set; }
		public T Stepsize { get; set; }
		public string Unit { get; set; }
		public bool Cyclic { get; set; }
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
		public string Subtype { get; set; }
		public string Filemask { get; set; }
		public string MaxChars { get; set; }
		public bool Multiline { get; set; }
		
		public TOUIString()
		: base ("String", "")
		{
		}
	}
	
	public class TOUIColor: ValueDefinition
	{
		public string Subtype { get; set; }
		
		public TOUIColor(string subtype)
		: base ("Color", Color.Black)
		{
			Subtype = subtype;
		}
	}
	
	public class TOUIEnum: ValueDefinition
	{
		public string[] Entries { get; set; }
		
		public TOUIEnum(string[] entries)
		: base ("Enum", 0)
		{
			Entries = entries;
		}
	}
	
	public class Vector2<T>: ValueDefinition
	{
		public T Subtype { get; set; }
		
		public Vector2(T subtype)
		: base ("Vector", null)
		{
			Subtype = subtype;
		}
	}
	
	public class TOUIDictionary<K, V>: ValueDefinition
	{
		public K KeyDefinition { get; set; }
		public V ValueDefinition { get; set; }
		
		public TOUIDictionary(K keyDefinition, V valueDefinition)
		: base ("Dictionary", null)
		{
			KeyDefinition = keyDefinition;
			ValueDefinition = valueDefinition;
		}
	}
}