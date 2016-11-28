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
		: base ("Color", Color.FromArgb(0))
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
	
	public class TOUIVector2<X, Y>: ValueDefinition
	where X: ValueDefinition
	where Y: ValueDefinition
	{
		public X XSubtype { get; set; }
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
		public X XSubtype { get; set; }
		public Y YSubtype { get; set; }
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
		public X XSubtype { get; set; }
		public Y YSubtype { get; set; }
		public Z ZSubtype { get; set; }
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