#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using VVVV.Core.Logging;
using Newtonsoft.Json;
#endregion usings

namespace TOUI
{
        public abstract class Base: IDisposable
        {
                public ILogger Logger {get; set;}
                
                protected Packet Pack(Command command, Value value)
                {
                        var packet = new Packet();
                        packet.Command = command;
                        packet.Data = value;
                        
                        return packet;
                }
                
                protected Packet Pack(Command command, string id)
                {
                        var packet = new Packet();
                        packet.Command = command;
                        packet.Data = new Number(id, null, null);
                        
                        return packet;
                }
                
                protected Packet Pack(Command command)
                {
                        var packet = new Packet();
                        packet.Command = command;
                        packet.Data = new Number("", null, null);
                        
                        return packet;
                }
                
                public virtual void Dispose()
                {
                        Logger = null;
                }
        }
        
        public class Server: Base 
        {
                private IServerTransporter FTransporter;
                public IServerTransporter Transporter
                { 
                        get { return FTransporter; }

                        set 
                        {
                                if (FTransporter != null)
                                        FTransporter.Dispose();
                                
                                FTransporter = value;	
                                FTransporter.Received = ReceiveCB;
                        }
                }
                
                public Action<Value> ValueUpdated;
                
                Dictionary<string, Value> FValues = new Dictionary<string, Value>();
                
                public string[] IDs 
                {
                        get { return FValues.Keys.ToArray(); }
                }
                
                public override void Dispose()
                {
                        if (FTransporter != null)
                                FTransporter.Dispose();
                }
                
                public bool AddValue(Value value)
                {
                        var result = false;
                        if (!FValues.ContainsKey(value.Id))
                        {
                                FValues.Add(value.Id, value);
                                result = true;
                        }
                        
                        //dispatch to all clients via transporter
                        var packet = Pack(Command.Add, value);
                        Transporter.Send(packet);
                        
                        return result;
                }
                
                public bool UpdateValue(Value value)
                {
                        var result = false;
                        if (FValues.ContainsKey(value.Id))
                                FValues.Remove(value.Id);
                        
                        FValues.Add(value.Id, value);
                        result = true;
                        
                        //dispatch to all clients via transporter
                        
                        return result;
                }
                
                public bool RemoveValue(string id)
                {
                        var result = FValues.Remove(id);
                        
                        //dispatch to all clients via transporter
                        var packet = Pack(Command.Remove, id);
                        Transporter.Send(packet);
                        
                        return result;
                }
                
                #region Transporter
                void ReceiveCB(Packet packet)
                {
                        Logger.Log(LogType.Debug, "Server received packet from Client");
                        
                        switch (packet.Command)
                        {
                                case Command.Update:
                                //inform the application
                                if (ValueUpdated != null)
                                        ValueUpdated(packet.Data);
                                break;
                                
                                case Command.Init:
                                //client requests all values
                                foreach (var value in FValues.Values)
                                        Transporter.Send(Pack(Command.Add, value));
                                break;
                        }
                }
                #endregion
        }
        
        public class Client: Base
        {
                private IClientTransporter FTransporter;
                public IClientTransporter Transporter
                { 
                        get { return FTransporter; }

                        set 
                        {
                                if (FTransporter != null)
                                        FTransporter.Dispose();
                                
                                FTransporter = value;	
                                FTransporter.Received = ReceiveCB;
                        }
                }
                
                public override void Dispose()
                {
                        if (FTransporter != null)
                                FTransporter.Dispose();
                }
                
                public void Init()
                {
                        Logger.Log(LogType.Debug, "Client requests Init");
                        Transporter.Send(Pack(Command.Init));
                }
                
                public Action<Value> ValueAdded;
                public Action<Value> ValueUpdated;
                public Action<string> ValueRemoved;
                
                void ReceiveCB(Packet packet)
                {
                        switch (packet.Command)
                        {
                                case Command.Add:
                                //inform the application
                                if (ValueAdded != null)
                                        ValueAdded(packet.Data);
                                break;
                                
                                case Command.Update:
                                //inform the application
                                if (ValueUpdated != null)
                                        ValueUpdated(packet.Data);
                                break;
                                
                                case Command.Remove:
                                //inform the application
                                if (ValueRemoved != null)
                                        ValueRemoved(packet.Data.Id);
                                break;
                        }
                }
        }
        
        public interface ISerializer 
        {
                byte[] Serialize(Packet packet);
                Packet Deserialize(byte[] bytes);
        }
        
        public class JsonSerializer: ISerializer
        {
                public byte[] Serialize(Packet packet)
                {
                        var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
                        var json = JsonConvert.SerializeObject(packet/*, serializerSettings*/);
                        return Encoding.UTF8.GetBytes(json);	
                }
                
                public Packet Deserialize(byte[] bytes)
                {
                        var json = Encoding.UTF8.GetString(bytes);
                        
                        //first decode as dynamic to check for the datatype
                        dynamic v = JsonConvert.DeserializeObject(json);

                        var packet = new Packet();
                        Command c;
                        Command.TryParse(v.Command.ToString(), out c);
                        packet.Command = c;
                        
                        //decode to specific datatype
                        if (v.Data.Type == "Number")
                                packet.Data = JsonConvert.DeserializeObject<Number>(v.Data.ToString());
                        else if (v.Data.Type == "String")
                                packet.Data = JsonConvert.DeserializeObject<_String>(v.Data.ToString());
                        
                        return packet;
                }
        }
        
        public interface IServerTransporter: IDisposable
        {
                ISerializer Serializer {get; set;}
                void Send(Packet packet);
                Action<Packet> Received {get; set;}
        }
        
        public interface IClientTransporter: IDisposable
        {
                ISerializer Serializer { get; set; }
                void Send(Packet packet);
                Action<Packet> Received {get; set;}
        }
        
        public class UDPServerTransporter: IServerTransporter
        {
                private UdpClient FUDPSender;
                private UdpClient FUDPReceiver;
                private Thread FThread;
                private bool FListening;
                
                public ISerializer Serializer {get; set;}
                
                public UDPServerTransporter(string remoteHost, int remotePort, int localPort)
                {
                        FUDPSender = new UdpClient(remoteHost, remotePort);
                        FUDPReceiver = new UdpClient(localPort);
                        FListening = true;
                        FThread = new Thread(new ThreadStart(ListenToUDP));
                        FThread.Start();
                }
                
                public void Dispose()
                {
                        if (FThread != null && FThread.IsAlive)
                        {
                                FListening = false;
                                //FThread.Join();
                        }

                        if (FUDPSender != null)
                        {
                                FUDPSender.Close();
                                FUDPSender.Dispose();
                        }	
                        
                        if (FUDPReceiver != null)
                        {
                                FUDPReceiver.Close();
                                FUDPReceiver.Dispose();
                        }	
                }
                
                public void Send(Packet packet)
                {
                        //send to all clients
                        var bytes = Serializer.Serialize(packet);
                        FUDPSender.Send(bytes, bytes.Length);
                }
                
                public Action<Packet> Received {get; set;}
                
                private void ListenToUDP()
                {
                        while(FListening)
                        {
                                try
                                {
                                        IPEndPoint ipEndPoint = null;
                                        var bytes = FUDPReceiver.Receive(ref ipEndPoint);
                                        
                                        if (bytes.Length > 0 && Received != null && Serializer != null)
                                        {
                                                Received(Serializer.Deserialize(bytes));
                                        }
                                }
                                catch (Exception e)
                                {
                                        //MessageBox.Show("UDP receive error on server: " + e.Message);
                                }
                        }
                }
        }
        
        public class UDPClientTransporter: IClientTransporter
        {
                private UdpClient FUDPSender;
                private UdpClient FUDPReceiver;
                private Thread FThread;
                private bool FListening;
                
                public ISerializer Serializer {get; set;}
                
                public UDPClientTransporter(string remoteHost, int remotePort, int localPort)
                {
                        FUDPSender = new UdpClient(remoteHost, remotePort);
                        FUDPReceiver = new UdpClient(localPort);
                        FListening = true;
                        FThread = new Thread(new ThreadStart(ListenToUDP));
                        FThread.Start();
                }
                
                public void Dispose()
                {
                        if (FThread != null && FThread.IsAlive)
                        {
                                FListening = false;
                                //FThread.Join();
                        }

                        if (FUDPSender != null)
                        {
                                FUDPSender.Close();
                                FUDPSender.Dispose();
                        }	
                        
                        if (FUDPReceiver != null)
                        {
                                FUDPReceiver.Close();
                                FUDPReceiver.Dispose();
                        }	
                }
                
                public void Send(Packet packet)
                {
                        var bytes = Serializer.Serialize(packet);
                        var r = FUDPSender.Send(bytes, bytes.Length);
                }
                
                public Action<Packet> Received {get; set;}
                
                private void ListenToUDP()
                {
                        while(FListening)
                        {
                                try
                                {
                                        IPEndPoint ipEndPoint = null;
                                        var bytes = FUDPReceiver.Receive(ref ipEndPoint);
                                        if (Received != null && Serializer != null)
                                                Received(Serializer.Deserialize(bytes));
                                }
                                catch (Exception e)
                                {
                                        //FLogger.Log(LogType.Debug, "UDP: " + e.Message);
                                }
                        }
                }
        }
        
        public enum Command { Add, Update, Remove, Init };
        
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
