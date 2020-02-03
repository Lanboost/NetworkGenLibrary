interface NetClass {
	public void WriteHeader(BinaryWriter writer, object var);
}

class NetObject {
	public uint netId;
}

class NetReader {
	Dictionary<uint, object> netObjects = new Dictionary<uint, object>();
		
	public static void ReadTick(BinaryReader reader) {
		uint netType = reader.ReadUInt32();
		uint netId = reader.ReadUInt32();
		bool fullObject = netId == UInt32.MaxValue;
		
		{{#objects}}
		{{#netobject}}
		if(netType == {{netType}}) {
			if(fullObject) {			
				var obj = {{name}}.ReadObject();
				netObjects.Add(obj.netId, obj);
			}
			else {
				(({{name}}) netObjects[netId]).Read(reader);
			}
		}
		{{/netobject}}
		{{/objects}}
	
	}
}



{{#objects}}
{{#netobject}}
class {{name}} : NetObject, NetClass  {
{{/netobject}}
{{^netobject}}
class {{name}} : NetClass  {
{{/netobject}}


	{{#vars}}
	{{type}} {{name}};
	{{/vars}}
	
	{{#vars}}
	public {{type}} Get{{name}}() {
		return this.{{name}};
	}
	{{/vars}}
	
	public void ReadObject(BinaryWriter reader) {
		{{#vars}}
		{{#primitive}}
		this.{{name}} = reader.Read{{readtype}}();
		{{/primitive}}
		{{^primitive}}
		var isnull = reader.ReadByte();
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
		{{/vars}}
	}
	
	public void Read(BinaryWriter reader) {
		var index = reader.ReadByte();
	
		{{#vars}}
		if(index == {{index}}) {
			{{#primitive}}
			this.{{name}} = reader.Read{{readtype}}();
			{{/primitive}}
			{{^primitive}}
			var isnull = reader.ReadByte();
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
		{{/vars}}
	
	}
	
	
}

{{/objects}}