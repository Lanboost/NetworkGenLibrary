mergeInto(LibraryManager.library, {

	jsconnect: function(url) {
		var str = Pointer_stringify(url);
		error = false;
		connected = false;
		messages = [];
	
	
		socket = new WebSocket(str);
		socket.binaryType = 'arraybuffer';
		// Connection opened
		socket.addEventListener('open', function (event) {
		console.log("open");
			connected = true;
		});

		// Listen for errors
		socket.addEventListener('error', function (event) {
		console.log("error");
			error = true;
		});

		// Listen for close
		socket.addEventListener('close', function (event) {
			console.log("close");
			connected = false;
		});

		// Listen for messages
		socket.addEventListener('message', function (e) {
			console.log("Message");
			// Todo: handle other data types?
			if (e.data instanceof Blob)
			{
				var reader = new FileReader();
				reader.addEventListener("loadend", function() {
					var array = new Uint8Array(reader.result);
					messages.push(array);
				});
				reader.readAsArrayBuffer(e.data);
			}
			else if (e.data instanceof ArrayBuffer)
			{
				var array = new Uint8Array(e.data);
				messages.push(array);
			}
		});
		if(socket.readyState != socket.CONNECTING) {
			console.log("Readystate is:"+socket.readyState);
		}
		
		console.log("connected");
	},

	jsisConnected: function() {
		
		return connected;
	},

	jsisError: function() {
		
		return error;
	},	
	jspollLength: function() {
		if(messages.length == 0){
			return 0;
		}
		return messages[0].length;
	},

	jspollData: function (ptr, length) {
		if (messages.length == 0)
			return 0;
		if (messages[0].length > length)
			return 0;
		len = messages[0].length;
		HEAPU8.set(messages[0], ptr);
		messages = messages.slice(1);
		return len;
	},

	jssendData: function(data, length) {
		socket.send(HEAPU8.buffer.slice(data, data+length));
	}

});




