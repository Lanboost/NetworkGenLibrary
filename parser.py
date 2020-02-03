import pystache

class lexer():

    def __init__(self, data):
        self.symbols = []
        self.line = []
        self.all_symbols = []
        
        curr = ""
        curr_all = ""
        currline = 0
        for c in data:
            if c in [" ","\t","\r","\n"]:        
                if curr_all != None:
                    curr_all += c
                else:
                    curr_all = c
            
                if curr != "":
                    self.symbols += [curr]
                    self.line += [currline]
                    curr = ""
                    
                    
                if c == "\n":
                    currline += 1
            elif c in [".",",","{","}","[","]","<",">",";","(",")","=","!","#"]:

                if curr != "":
                    self.symbols += [curr]
                    self.line += [currline]

                if curr_all != None:
                    self.all_symbols += [curr_all]
                else:
                    self.all_symbols += [""]
                curr_all = ""
                curr = ""
                self.symbols += [c]
                self.line += [currline]
                
                
            else:
                if curr_all != None:
                    self.all_symbols += [curr_all]
                    curr_all = None
                curr += c
                
        self.pos = 0
    
    def get(self):
        if self.pos < 0 or self.pos >= len(self.symbols):
            return '\0'
        return self.symbols[self.pos]
    
    def next(self):
        self.pos+=1
        return self.get()
        
    def back(self):
        self.pos-=1
        return self.get()
    
    def set(self, pos):
        self.pos = pos
        return self.get()
        
    def get_line(self):
        return self.line[self.pos]
        
    def get_all_symbols(self):
        return self.all_symbols[self.pos]
        

class Node:
    def __init__(self, left, right, name):
        self.left=left
        self.right=right
        self.name=name

def pr(node):
    a = "("
    if node.left!=None:
        a += pr(node.left)
    a += " " + node.name + " "
    if node.right != None:
        a += pr(node.right)
    a+=')'
    return a

class Parser:
    def __init__(self, s):
        self.data = s
        self.lex = lexer(self.data)
        self.current = self.lex.get()

    def accept(self, c):
        if self.current == c:
            self.next()
            return True
        return False
        
    def next(self):
        self.current = self.lex.next()
        return self.current
        
    def back(self):
        self.current = self.lex.back()
        return self.current
        
    def expect(self, c):
        if self.current == c:
            self.next()
            return True
        raise Exception("Unexpected character", self.current, "expected", c, "on line",self.lex.get_line())
                
    def reset(self):
        self.current = self.lex.set(0)
        
    def variable_name(self):
        l = self.current
        self.next()
        return l
        
    def variable_array(self):
        if self.accept('['):
            self.expect("]")
            return True
            
        return False
        
    def variable_type(self, var_type_names):
        if self.accept('int') or self.accept('Int'):
            return 'int'
        if self.accept('string') or self.accept('String'):
            return 'string'
        if self.accept('float') or self.accept('Float'):
            return 'float'
        if self.accept('dict') or self.accept('Dict'):
            self.expect("<")
            t1 = self.variable_type()
            self.expect(",")
            t2 = self.variable_type()
            self.expect(">")
            return ("dict", t1, t2)
        if self.accept('list') or self.accept('List'):
            self.expect("<")
            t1 = self.variable_type()
            self.expect(">")
            return ("dict", t1)
        for var_type_name in var_type_names:
            if self.accept(var_type_name):
                return var_type_name
                
        if self.accept("#"):
            return self.variable_name()
            
        raise Exception("Unexpected token", self.current, "expected a type")
        
    def variable_end(self):
        self.expect(";")
        
    def variable_side(self):
        self.expect(",")
        if self.accept('Server'):
            return "server"
        elif self.accept('Client'):
            return "client"
        raise Exception("Expected side variable ('Server','Client')")
        
    def variable(self, var_type_names):
        t = self.variable_type(var_type_names)
        
        is_array = self.variable_array()
        
        name = self.variable_name()
        
        side = None
        try:
            side = self.variable_side()
        except:
            pass
        
        o = (t, is_array, name, side)
        self.variable_end()
        return o
        
    def object_name(self):
        l = self.current
        self.next()
        return l
        
    def object_netobject(self):
        if self.accept(","):
            if self.accept("NetObject"):
                return True
        return False
        
    def object_variables(self, var_type_names):
        vars = []
        while True:
            try:
                o = self.variable(var_type_names)
                vars += [o]
            except:
                break
        return vars
        
    def object_code_object(self):
        
        direct_copy = self.accept("#")
        if direct_copy:
            code = self.get_code_block()
            
            return (direct_copy, code)
        
    def object_variable_function(self):
        p = self.lex.pos
        try:
            variable = self.current
            self.next()
            self.expect(".")
            function = self.current
            self.next()
            self.expect("=")
            
            code = self.object_code_object()
        except:
            self.current = self.lex.set(p)
            raise Exception("")
        
        return (variable,function, code)
     
    def object_variable_functions(self):
        funcs = []
        while True:
            try:
                funcs += [self.object_variable_function()]
            except:
                return funcs
        
    def object(self, var_type_names):    
        name = self.object_name()
        is_netobject = self.object_netobject()
        self.expect("{")
        vars = self.object_variables(var_type_names)
        
        funcs = self.object_variable_functions()
        
        
        self.expect("}")
        return (name, is_netobject, vars, funcs)
        
    def get_code_block(self):
        code = []
        self.expect("{")
        ind = 1
        while True:
            code += [self.lex.get_all_symbols()]
        
            if self.accept("{"):
                code += ["{"]
                ind += 1
            elif self.accept("}"):
                code += ["}"]
                ind -= 1
            else:
                code += [self.current]
                self.next()
                
            if ind == 0:
                return code[:-1]
            
            if self.current == '\0':
                raise Exception(f"Unexpected end of file, expected '}}' is {ind} steps in, starting on line, name was {name}")
        
    def object_names(self):
        names = []
        while True:
            name = self.object_name()
            if name == '\0':
                self.reset()
                return names
            names += [name]
            
            self.object_netobject()
            
            self.get_code_block()
                
    
    def objects(self):
        var_type_names = self.object_names()
        
        obj = []
        while True:
            o = self.object(var_type_names)
            obj += [o]
            if self.current == '\0':
                return obj



def print_class_vars(data, type,is_array, name):
    if type in ['int','string','float']:
        if is_array:
            print("self.",name,"=[]")
        else:
            print("self.",name)
    else:
        if is_array:
            print("self.",name,"=[]")
        else:
            for d in data:
                if d[0] == type:
                    for v in d[2]:
                        print_class_vars(data, v[0], v[1], name+"_"+v[2])
        
def is_primative(type):
    return type in ['int','string','float']

def read_file(filename):
    with open(filename, "r") as f:
        return f.read()

def primative_to_binary_reader(type):
    d = {
        'int':'Int32',
        'string':'String',
        'float':'Float',
    }
    if type in d:
        return d[type]
    return None

def render(objects):

    

    net_type = 0
    oo = []
    for o in objects:
        vars = []
        for index,v in enumerate(o[2]):
            vars += [{'name':v[2],'type':v[0], 'index':index, 'primitive':is_primative(v[0]), 'readtype': primative_to_binary_reader(v[0])}]
        funcs = {"something":""} #init with something so it isn't false
        
        for f in o[3]:
            #print(" ".join(f[2][1]))
            funcs[f[1]] = "".join(f[2][1])
            
        elem = {'name':o[0], 'vars':vars, 'funcs':funcs}
        if o[1]:
            net_type += 1
            elem['netobject'] = {'netType':net_type}
            
        oo += [elem]


    return pystache.render(read_file('write_template.cs'), {'objects': oo}), pystache.render(read_file('read_template.cs'), {'objects': oo})
            

#gen = lexer(string)
p = Parser(read_file('network_code.txt'))
o = p.objects()
data_write, data_read = render(o)
with open("parser_out_write.cs","w") as f:
    f.write(data_write)
with open("parser_out_read.cs","w") as f:
    f.write(data_read)
        
        
        
        
'''
class Syncer {

    public static void Sync(Action<BinaryWriter> del, NetClass obj) {
        var sockets = obj.GetSyncSockets()
        foreach(var sock in sockets) {
            del(sock.tickStream);
        }
    }

}
'''
        
        
        