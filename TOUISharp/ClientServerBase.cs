using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VVVV.Core.Logging;

namespace TOUI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Command
    {
        [EnumMember(Value = "add")]
        Add,

        [EnumMember(Value = "update")]
        Update,

        [EnumMember(Value = "remove")]
        Remove,

        [EnumMember(Value = "init")]
        Init
    };

    public abstract class Base: IDisposable
	{
		public ILogger Logger { get; set; }
		public ISerializer Serializer { get; set; }
		
		protected byte[] Pack(Command command, Parameter parameter)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Data = parameter;
			
			return Serializer.Serialize(packet);
		}
		
		protected byte[] Pack(Command command, string id)
		{
			var packet = new Packet();
			packet.Command = command;
			packet.Data = new Parameter(id);
			
			return Serializer.Serialize(packet);
		}
		
		protected byte[] Pack(Command command)
		{
			var packet = new Packet();
			packet.Command = command;
			
			return Serializer.Serialize(packet);
		}
		
		public virtual void Dispose()
		{
			Logger = null;
		}
	}
}