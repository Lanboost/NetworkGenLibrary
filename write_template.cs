interface NetClass {
    public List<Socket> GetSyncSockets();
    public void WriteHeader(BinaryWriter writer, object var);
}

class NetObject {
    public uint netId;
}

class Socket
{
	public uint pid;
	public BinaryWriter tickStream = new BinaryWriter();

}

class Main
{
	public static List<Socket> players;

}

class PlayerBuilder {
	public Player build()
	{
		var p = new Player();
		p.SetSocket(v);
	}

}


{{#objects}}
{{#netobject}}
class {{name}} : NetObject, NetClass  {
{{/netobject}}
{{^netobject}}
class {{name}} : NetClass  {
{{/netobject}}

    {{^netobject}}
    NetClass parent;
    {{/netobject}}

    {{#vars}}
    {{type}} {{name}};
    {{/vars}}
    
    {{#vars}}
    public {{type}} Get{{name}}() {
        return this.{{name}};
    }
    
    public void Set{{name}}({{type}} value) {
        this.PreUpdate();
        this.{{name}} = value;
        this.PostUpdate();
        this.Sync(delegate(BinaryWriter w) {
            
            {{#netobject}}
            writer.Write({{netType}});
            writer.Write(netId);
            {{/netobject}}
            {{^netobject}}
            this.parent.WriteHeader(w, this);
            {{/netobject}}
            
            
            w.write((byte){{index}});
            {{#primitive}}
            w.write((byte)value);
            {{/primitive}}
            {{^primitive}}
            if(this.{{name}} == null) {
                w.write((byte)0);
            }
            else {
                w.write((byte)1);
                this.{{name}}.write(w);
            }
            {{/primitive}}
        }, this);
    }
    {{/vars}}
    
    public void write(Socket s, BinaryWriter w) {
        {{#netobject}}
        writer.Write({{netType}});
        writer.Write(UInt32.MaxValue);
        writer.Write(netId);
        {{/netobject}}
    
        {{#vars}}
        {{#primitive}}
        w.Write(this.{{name}});
        {{/primitive}}
        {{^primitive}}
        if(this.{{name}} == null) {
            w.write((byte)0);
        }
        else {
            if(w.SyncFilter(s)) {
                w.write((byte)1);
                this.{{name}}.write(w);
            }
            else {
                w.write((byte)0);
            }
        }
        {{/primitive}}
        {{/vars}}
    }
    
    {{#funcs}}
    {{#PreUpdate}}
    void PreUpdate() { {{{ . }}} }
    {{/PreUpdate}}
    {{^PreUpdate}}
    void PreUpdate() {}
    {{/PreUpdate}}
    
    {{#PostUpdate}}
    void PostUpdate() { {{{ . }}} }
    {{/PostUpdate}}
    {{^PostUpdate}}
    void PostUpdate() {}
    {{/PostUpdate}}
    
    {{#SyncFilter}}
    bool SyncFilter(Socket socket) { {{{ . }}} }
    {{/SyncFilter}}
    {{^SyncFilter}}
    bool SyncFilter(Socket socket) { return true;}
    {{/SyncFilter}}
    
    {{#GetSyncSockets}}
    List<Socket> GetSyncSockets() { {{{ . }}} }
    {{/GetSyncSockets}}
    {{^GetSyncSockets}}
    List<Socket> GetSyncSockets() {return this.parent.GetSyncSockets();}
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
            writer.write({{index}});
        }
        {{/primitive}}
        {{/vars}}
    }
    
    public void Sync(Action<BinaryWriter> del, NetClass obj) {
        var sockets = obj.GetSyncSockets();
        foreach(var sock in sockets) {
            del(sock.tickStream);
        }
    }
    
}

{{/objects}}