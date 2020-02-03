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
			uint netType = reader.ReadUInt32();
			uint netId = reader.ReadUInt32();
			bool fullObject = netId == UInt32.MaxValue;
			
			{{#objects}}
			{{#netobject}}
			if(netType == {{netType}}) {
				if(fullObject) {	
					var obj = new {{name}}();
					obj.ReadObject(reader);
					netObjects.Add(obj.netId, obj);
				}
				else {
					(({{name}}) netObjects[netId]).Read(reader);
				}
			}
			{{/netobject}}
			{{/objects}}
		
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
		{{type}} {{name}};
		{{/server_only}}
		{{/vars}}
		
		{{#vars}}
		{{^server_only}}
		public {{type}} Get{{name}}() {
			return this.{{name}};
		}
		{{/server_only}}
		{{/vars}}
		
		public void ReadObject(BinaryReader reader) {
			{{#vars}}
			{{^server_only}}
			{
				{{#primitive}}
				this.{{name}} = {{readtype}}();
				{{/primitive}}
				{{^primitive}}
				var isnull = reader.ReadByte() == 0;
				if(isnull) {
					this.{{name}} = null;
				}
				else {
					if(this.{{name}} == null) {
						this.{{name}} = new {{type}}();
					}
					this.{{name}}.ReadObject(reader);
				}
				{{/primitive}}
			}
			{{/server_only}}
			{{/vars}}
		}
		
		public void Read(BinaryReader reader) {
			var index = reader.ReadByte();
		
			{{#vars}}
			{{^server_only}}
			if(index == {{index}}) {
				{{#primitive}}
				this.{{name}} = {{readtype}}();
				{{/primitive}}
				{{^primitive}}
				var isnull = reader.ReadByte() == 0;
				if(isnull) {
					this.{{name}} = null;
				}
				else {
					if(this.{{name}} == null) {
						this.{{name}} = new {{type}}();
					}
					this.{{name}}.Read(reader);
				}
				{{/primitive}}
			}
			{{/server_only}}
			{{/vars}}
		
		}
		
		
	}

	{{/objects}}
}