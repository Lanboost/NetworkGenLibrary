using System.Collections.Generic;
using System.IO;
using System;

namespace NetServer {
	public interface NetClass {
		List<Socket> GetSyncSockets();
		void WriteHeader(BinaryWriter writer, object var);
		void Write(Socket s, BinaryWriter writer);
	}

	public class NetObject {
		public uint netId;
	}

	public class Socket
	{
		public uint pid;
		public BinaryWriter tickStream = new BinaryWriter(new MemoryStream());
		
		public NetClass parent;

	}

	public class Main
	{
		public static List<Socket> sockets = new List<Socket>();
		public static Dictionary<uint, Player> players = new Dictionary<uint, Player>();

	}

	public class PlayerBuilder {
		public Player build()
		{
			var p = new Player();
			p.SetSocket(null);
			return p;
		}

	}


	{{#objects}}
	{{#netobject}}
	public class {{name}} : NetObject, NetClass  {
	{{/netobject}}
	{{^netobject}}
	public class {{name}} : NetClass  {
	{{/netobject}}

		{{^netobject}}
		public NetClass parent;
		{{/netobject}}

		{{#vars}}
		{{type}} {{name}};
		{{/vars}}
		
		public {{name}}() {
			
		}
		
		public {{name}}({{#vars}}{{#first}}{{type}} {{name}}{{/first}}{{^first}}, {{type}} {{name}}{{/first}}{{/vars}}) {
			{{#vars}}
			this.{{name}} = {{name}};
			{{/vars}}
		}
		
		
		{{#vars}}
		public void Init{{name}}({{type}} value) {
			this.{{name}} = value;
			{{^primitive}}
			this.{{name}}.parent = this;
			{{/primitive}}
		}
		{{/vars}}
		
		{{#vars}}
		public {{type}} Get{{name}}() {
			return this.{{name}};
		}
		{{#server_only}}
		public void Set{{name}}({{type}} value) {
			this.{{name}} = value;
		}
		{{/server_only}}
		{{^server_only}}
		public void Set{{name}}({{type}} value) {
			this.PreUpdate();
			this.{{name}} = value;
			{{^primitive}}
			this.{{name}}.parent = this;
			{{/primitive}}
			
			this.PostUpdate();
			this.Sync(delegate(Socket s, BinaryWriter writer) {
				
				{{#netobject}}
				writer.Write({{netType}});
				writer.Write(netId);
				{{/netobject}}
				{{^netobject}}
				this.parent.WriteHeader(writer, this);
				{{/netobject}}
				
				
				writer.Write((byte){{index}});
				
				{{#primitive}}
				writer.Write(value);
				{{/primitive}}
				{{^primitive}}
				if(this.{{name}} == null) {
					writer.Write((byte)0);
				}
				else {
					writer.Write((byte)2);
					this.{{name}}.Write(s, writer);
				}
				{{/primitive}}
			}, this);
		}
		{{/server_only}}
		{{/vars}}
		
		public void Write(Socket s, BinaryWriter writer) {
			{{#netobject}}
			writer.Write({{netType}});
			writer.Write(UInt32.MaxValue);
			writer.Write(netId);
			{{/netobject}}
		
			{{#vars}}
			{{^server_only}}
			
			{{#primitive}}
			writer.Write(this.{{name}});
			{{/primitive}}
			{{^primitive}}
			if(this.{{name}} == null) {
				writer.Write((byte)0);
			}
			else {
				if(this.{{name}}.SyncFilter(s)) {
					writer.Write((byte)1);
					this.{{name}}.Write(s, writer);
				}
				else {
					writer.Write((byte)0);
				}
			}
			{{/primitive}}
			{{/server_only}}
			{{/vars}}
		}
		
		{{#funcs}}
		{{#PreUpdate}}
		public void PreUpdate() { {{{ . }}} }
		{{/PreUpdate}}
		{{^PreUpdate}}
		public void PreUpdate() {}
		{{/PreUpdate}}
		
		{{#PostUpdate}}
		public void PostUpdate() { {{{ . }}} }
		{{/PostUpdate}}
		{{^PostUpdate}}
		public void PostUpdate() {}
		{{/PostUpdate}}
		
		{{#SyncFilter}}
		public bool SyncFilter(Socket socket) { {{{ . }}} }
		{{/SyncFilter}}
		{{^SyncFilter}}
		public bool SyncFilter(Socket socket) { return true;}
		{{/SyncFilter}}
		
		{{#GetSyncSockets}}
		public List<Socket> GetSyncSockets() { {{{ . }}} }
		{{/GetSyncSockets}}
		{{^GetSyncSockets}}
		public List<Socket> GetSyncSockets() {return this.parent.GetSyncSockets();}
		{{/GetSyncSockets}}
		
		{{/funcs}}
		
		public void WriteHeader(BinaryWriter writer, object var) {
			{{#netobject}}
			writer.Write({{netType}});
			writer.Write(netId);
			{{/netobject}}
			{{^netobject}}
			this.parent.WriteHeader(writer, this);
			{{/netobject}}
		
			{{#vars}}
			{{^primitive}}
			if(this.{{name}} == var) {
				writer.Write((byte){{index}});
				writer.Write((byte)1);
			}
			{{/primitive}}
			{{/vars}}
		}
		
		public void Sync(Action<Socket, BinaryWriter> del, NetClass obj) {
			var sockets = obj.GetSyncSockets();
			foreach(var sock in sockets) {
				del(sock, sock.tickStream);
			}
		}
		
	}

	{{/objects}}
}