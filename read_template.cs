using System.Collections.Generic;
using System.IO;
using System;

namespace NetClient {
	public interface NetClass {
		void ReadObject(BinaryReader reader);
		void Read(BinaryReader reader);
	}

	public class NetObject {
		public uint netId;
	}

	public class NetReader {
		public Dictionary<uint, object> netObjects = new Dictionary<uint, object>();
			
		public void ReadTick(BinaryReader reader) {
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				uint netType = reader.ReadUInt32();
				uint netId = reader.ReadUInt32();
				bool fullObject = netId == UInt32.MaxValue;
				
				{{#objects}}
				{{#netobject}}
				if(netType == {{netType}}) {
					if(fullObject) {	
						netId = reader.ReadUInt32();
						var obj = new {{name}}();
						Console.WriteLine($"Full read netType {netType} netId {netId}");
						obj.ReadObject(reader);
						obj.netId = netId;
						netObjects.Add(obj.netId, obj);
					}
					else {
						Console.WriteLine($"Read netType {netType} netId {netId}");
						(({{name}}) netObjects[netId]).Read(reader);
					}
				}
				{{/netobject}}
				{{/objects}}
			}
			reader.BaseStream.SetLength(0);
		
		}
		
		public T Get<T>(uint id) {
			if(netObjects[id].GetType() == typeof(T)) {
				return (T) netObjects[id];
			}
			return default(T);
		}
	}



	{{#objects}}
	{{#netobject}}
	public class {{name}} : NetObject, NetClass  {
	{{/netobject}}
	{{^netobject}}
	public class {{name}} : NetClass  {
	{{/netobject}}


		{{#vars}}
		{{^server_only}}
		{{type}}{{#array}}[]{{/array}} {{name}};
		{{/server_only}}
		{{/vars}}
		
		{{#vars}}
		{{^server_only}}
		public {{type}}{{#array}}[]{{/array}} Get{{name}}() {
			return this.{{name}};
		}
		{{/server_only}}
		{{/vars}}
		
		public void ReadObject(BinaryReader reader) {
			{{#vars}}
			{{^server_only}}
			{
				{{#array}}
				var isnull = reader.ReadByte() == 0;
				if(isnull) {
					this.{{name}} = null;
					Console.WriteLine($"\t Reading array {{name}} value is null");
				}
				else {
					var len = reader.ReadInt32();
					this.{{name}} = new {{type}}[len];
					Console.WriteLine($"\t Reading array {{name}} length is {len}");
					for(int i=0; i<len; i++) {
						var elem_isnull = reader.ReadByte() == 0;
						if(!elem_isnull) {
							this.{{name}}[i] = new {{type}}();
							this.{{name}}[i].ReadObject(reader);
						}
					}
				}
				
				{{/array}}
				{{^array}}
				{{#primitive}}
				this.{{name}} = {{readtype}}();
				Console.WriteLine($"\t Reading variable {{name}} value is {this.{{name}}}");
				{{/primitive}}
				{{^primitive}}
				var isnull = reader.ReadByte() == 0;
				if(isnull) {
					this.{{name}} = null;
					Console.WriteLine($"\t Reading variable {{name}} value is null");
				}
				else {
					if(this.{{name}} == null) {
						this.{{name}} = new {{type}}();
					}
					Console.WriteLine($"\t Reading variable {{name}} value is {this.{{name}}}");
					this.{{name}}.ReadObject(reader);
				}
				{{/primitive}}
				{{/array}}
			}
			{{/server_only}}
			{{/vars}}
		}
		
		public void Read(BinaryReader reader) {
			var index = reader.ReadByte();
			{{#vars}}
			{{^server_only}}
			if(index == {{index}}) {
				{{#array}}
				var isnull = reader.ReadByte();
				if(isnull == 0) {
					this.{{name}} = null;
					Console.WriteLine($"\t Reading array index {{name}} value is null");
				}
				else if(isnull == 1) {
					var elem_index = reader.ReadByte();
					var elem_isnull = reader.ReadByte();
					Console.WriteLine($"\t Reading array {{name}} elem_index {elem_index} elem_isnull {elem_isnull}");
					if(elem_isnull == 0) {
						this.{{name}}[elem_index] = null;
					}
					else if(elem_isnull == 1) {
						this.{{name}}[elem_index].Read(reader);
					}
					else if(elem_isnull == 2) {
						this.{{name}}[elem_index] = new {{type}}();
						this.{{name}}[elem_index].ReadObject(reader);
					}
				}
				else {
					var len = reader.ReadInt32();
					this.{{name}} = new {{type}}[len];
					Console.WriteLine($"\t Reading array {{name}} length is {len}");
					for(int i=0; i<len; i++) {
						var elem_isnull = reader.ReadByte() == 0;
						if(!elem_isnull) {
							this.{{name}}[i] = new {{type}}();
							this.{{name}}[i].ReadObject(reader);
						}
					}
				}
				{{/array}}
				{{^array}}
				{{#primitive}}
				this.{{name}} = {{readtype}}();
				Console.WriteLine($"\t Reading variable index {{name}} value is {this.{{name}}}");
				{{/primitive}}
				{{^primitive}}
				var isnull = reader.ReadByte();
				Console.WriteLine($"\t\t isnull is  {isnull}");
				if(isnull == 0) {
					this.{{name}} = null;
					Console.WriteLine($"\t Reading variable index {{name}} value is null");
				}
				else if(isnull == 1) {
					if(this.{{name}} == null) {
						this.{{name}} = new {{type}}();
					}
					Console.WriteLine($"\t Reading variable index {{name}} value is {this.{{name}}}");
					this.{{name}}.Read(reader);
				}
				else {
					if(this.{{name}} == null) {
						this.{{name}} = new {{type}}();
					}
					Console.WriteLine($"\t Reading variable index {{name}} value is {this.{{name}}}");
					this.{{name}}.ReadObject(reader);
				}
				{{/primitive}}
				{{/array}}
			}
			{{/server_only}}
			{{/vars}}
		
		}
		
		
	}

	{{/objects}}
}