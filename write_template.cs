using System.Collections.Generic;
using System.IO;
using System;
using System.Collections;

namespace NetServer {
	public interface NetClass {
		List<Socket> GetSyncSockets();
		void WriteHeader(BinaryWriter writer);
		void Write(Socket s, BinaryWriter writer);
		
		void SetParent(NetClass nClass);
		void SetParentIndex(int index);
	}

	public class NetObject {
		public uint netId;
	}

	public class Socket
	{
		public uint pid;
		public BinaryWriter tickStream = new BinaryWriter(new MemoryStream());
		
		NetClass parent;
		
		public void SetParent(NetClass nClass) {
			this.parent = nClass;
		}

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
	
	public class ArrayWrapperPrimitive<T> : ArrayWrapper<T>
	{
		public ArrayWrapperPrimitive() {}
		
		public ArrayWrapperPrimitive(T[] data):base(data) {}
		
		protected override void OnSetData()
		{
		}

		protected override void OnWrite(Socket s, BinaryWriter writer, int index)
		{
			if(typeof(T) == typeof(int)) {
				writer.Write((int)(object)data[index]);	
			}
			else if(typeof(T) == typeof(string)) {
				writer.Write((string)(object)data[index]);	
			}
			else if(typeof(T) == typeof(float)) {
				writer.Write((float)(object)data[index]);	
			}
		}
		
		protected override void OnValueSet(int index)
		{
			this.Sync(delegate(Socket s, BinaryWriter writer) {
				this.parent.WriteHeader(writer);
				writer.Write((byte)this.parent_index);
				writer.Write((byte)1);
				
				writer.Write((byte)index);
				
				
				
				if(this.data[index] == null) {
					writer.Write((byte)0);
				}
				else {
					writer.Write((byte)2);
					OnWrite(s, writer, index);
				}
			}, this);
		}
	}

	public class ArrayWrapperNonPrimitive<T> : ArrayWrapper<T> where T : NetClass
	{
		public ArrayWrapperNonPrimitive() {}
		
		public ArrayWrapperNonPrimitive(T[] data):base(data) {}
		
		protected override void OnSetData()
		{
			for(int i=0; i<data.Length; i++) {
				this.data[i].SetParent(this);
				this.data[i].SetParentIndex(i);
			}
		}

		protected override void OnWrite(Socket s, BinaryWriter writer, int index)
		{
			this.data[index].Write(s, writer);
		}
		
		protected override void OnValueSet(int index)
		{
			this.Sync(delegate(Socket s, BinaryWriter writer) {
				this.parent.WriteHeader(writer);
				writer.Write((byte)this.parent_index);
				writer.Write((byte)1);
				
				writer.Write((byte)index);
				
				
				
				if(this.data[index] == null) {
					writer.Write((byte)0);
				}
				else {
					writer.Write((byte)2);
					data[index].Write(s, writer);
				}
			}, this);
		}
	}
	
	public abstract class ArrayWrapper<T>:IList<T>, NetClass {
		protected T[] data;
		
		public ArrayWrapper() {}
		
		public ArrayWrapper(T[] data) {
			SetData(data);
		}

		protected NetClass parent;
		protected int parent_index;
		
		protected abstract void OnValueSet(int index);

		public T this[int index] { 
			get {
				return data[index];
			}
			set {
				data[index] = value;
			}
		}

		protected abstract void OnSetData();

		public void SetData(T[] data) {
			this.data = data;
			OnSetData();
		}

		public int Count { get { return data.Length; }}

		public bool IsReadOnly => throw new NotImplementedException();

		public void Add(T item)	{throw new NotImplementedException();}

		public void Clear()	{throw new NotImplementedException();}

		public bool Contains(T item){throw new NotImplementedException();}

		public void CopyTo(T[] array, int arrayIndex){throw new NotImplementedException();}
		
		public IEnumerator<T> GetEnumerator(){throw new NotImplementedException();}

		public int IndexOf(T item){	throw new NotImplementedException();}

		public void Insert(int index, T item){throw new NotImplementedException();}

		public bool Remove(T item){throw new NotImplementedException();}

		public void RemoveAt(int index) {throw new NotImplementedException();}

		IEnumerator IEnumerable.GetEnumerator() {throw new NotImplementedException();}

		protected abstract void OnWrite(Socket s, BinaryWriter writer, int index);

		public void Write(Socket s, BinaryWriter writer) {
			
			writer.Write(data.Length);
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == null)
				{
					writer.Write((byte)0);
				}
				else
				{
					writer.Write((byte)1);
					OnWrite(s, writer, i);
				}
			}
			
		}

		public void WriteHeader(BinaryWriter writer)
		{
			writer.Write((byte)parent_index);
		}

		public List<Socket> GetSyncSockets()
		{
			return this.parent.GetSyncSockets();
		}

		public void SetParent(NetClass nClass)
		{
			this.parent = nClass;
		}

		public void SetParentIndex(int index)
		{
			this.parent_index = index;
		}
		
		public void Sync(Action<Socket, BinaryWriter> del, NetClass obj) {
			var sockets = obj.GetSyncSockets();
			foreach(var sock in sockets) {
				del(sock, sock.tickStream);
			}
		}
	}



	{{#objects}}
	{{#netobject}}
	public class {{name}} : NetObject, NetClass  {
	{{/netobject}}
	{{^netobject}}
	public class {{name}} : NetClass  {
	{{/netobject}}
		
		{{#netobject}}
		public void SetParent(NetClass nClass) {}
		public void SetParentIndex(int index) {}
		{{/netobject}}
		{{^netobject}}
		NetClass parent;
		int parent_index;
		public void SetParent(NetClass nClass) { this.parent = nClass;}
		public void SetParentIndex(int index) { this.parent_index = index;}
		{{/netobject}}

		{{#vars}}
		{{#array}}
		ArrayWrapper< {{type}} > {{name}};
		{{/array}}
		{{^array}}
		{{type}} {{name}};
		{{/array}}
		{{/vars}}
		
		public {{name}}() {
			
		}
		
		public {{name}}({{#vars}}{{#first}}{{type}}{{#array}}[]{{/array}} {{name}}{{/first}}{{^first}}, {{type}}{{#array}}[]{{/array}} {{name}}{{/first}}{{/vars}}) {
			{{#vars}}
			{{#array}}
			if({{name}} != null) {
				this.{{name}} = new {{#primitive}}ArrayWrapperPrimitive{{/primitive}}{{^primitive}}ArrayWrapperNonPrimitive{{/primitive}}<{{type}}>({{name}});
				this.{{name}}.SetParent(this);
			}
			{{/array}}
			{{^array}}
			this.{{name}} = {{name}};
			{{^primitive}}
			this.{{name}}.SetParent(this);
			{{/primitive}}
			{{/array}}
			{{/vars}}
		}
		
		
		{{#vars}}
		public void Init{{name}}({{type}}{{#array}}[]{{/array}} value) {
			{{#array}}
			if(value != null) {
				this.{{name}} = new {{#primitive}}ArrayWrapperPrimitive{{/primitive}}{{^primitive}}ArrayWrapperNonPrimitive{{/primitive}}<{{type}}>(value);
				this.{{name}}.SetParent(this);
			}
			{{/array}}
			{{^array}}
			this.{{name}} = value;
			{{^primitive}}
			if(this.{{name}} != null) {
				this.{{name}}.SetParent(this);
			}
			{{/primitive}}
			{{/array}}
		}
		{{/vars}}
		
		{{#vars}}
		{{#array}}
		public ArrayWrapper<{{type}}> Get{{name}}() {
		{{/array}}
		{{^array}}
		public {{type}} Get{{name}}() {
		{{/array}}
			return this.{{name}};
		}
		
		public void Set{{name}}({{type}}{{#array}}[]{{/array}} value) {
			{{#server_only}}
			this.{{name}} = value;
			{{/server_only}}
			{{^server_only}}
			this.PreUpdate();
			
			{{#array}}
			if(value != null) {
				this.{{name}} = new {{#primitive}}ArrayWrapperPrimitive{{/primitive}}{{^primitive}}ArrayWrapperNonPrimitive{{/primitive}}<{{type}}>(value);
				this.{{name}}.SetParent(this);
				this.{{name}}.SetParentIndex({{index}});
			}
			{{/array}}
			{{^array}}
			this.{{name}} = value;
			{{^primitive}}
			this.{{name}}.SetParent(this);
			this.{{name}}.SetParentIndex({{index}});
			{{/primitive}}
			{{/array}}
			
			this.PostUpdate();
			this.Sync(delegate(Socket s, BinaryWriter writer) {
				
				{{#netobject}}
				writer.Write({{netType}});
				writer.Write(netId);
				{{/netobject}}
				{{^netobject}}
				this.parent.WriteHeader(writer);
				writer.Write((byte)this.parent_index);
				writer.Write((byte)1);
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
			{{/server_only}}
		}
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
				{{#array}}
				writer.Write((byte)1);
				this.{{name}}.Write(s, writer);
				{{/array}}
				{{^array}}
				if(this.{{name}}.SyncFilter(s)) {
					writer.Write((byte)1);
					this.{{name}}.Write(s, writer);
				}
				else {
					writer.Write((byte)0);
				}
				{{/array}}
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
		
		public void WriteHeader(BinaryWriter writer) {
			{{#netobject}}
			writer.Write({{netType}});
			writer.Write(netId);
			{{/netobject}}
			{{^netobject}}
			this.parent.WriteHeader(writer);
			writer.Write(this.parent_index);
			{{/netobject}}
			
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