#### About
A wip library for [WebSocket][1] servers, written in .net C#. Ability to send and receive in both bytes and strings synchronously. 
All sample websocket clients, written in javascript, can be found in their respective sample folders.

All message receives will be preceeded by fin then opcode, e.g. "11Hello World". Quoted from the [WebSocket Proposed Standard][3]:
```javscript
EXAMPLE: For a text message sent as three fragments, the first
      fragment would have an opcode of 0x1 and a FIN bit clear, the
      second fragment would have an opcode of 0x0 and a FIN bit clear,
      and the third fragment would have an opcode of 0x0 and a FIN bit
      that is set.
      
      Opcodes
      
      *  %x0 denotes a continuation frame
      *  %x1 denotes a text frame
      *  %x2 denotes a binary frame
      *  %x3-7 are reserved for further non-control frames
      *  %x8 denotes a connection close
      *  %x9 denotes a ping
      *  %xA denotes a pong
      *  %xB-F are reserved for further control frames
```

#### Example server
```csharp
static void Main(string[] args)
{
	Websocket websocket = new Websocket(IPAddress.Any, 8001);
	while (true) {
		Socket client = websocket.AcceptSocket();
		if (client != null) {
			websocket.SendString(client, "hello to you.");
			Thread.Sleep(20);
			if (client.Available > 0) {
				string s = websocket.ReceiveString(client);
				Console.WriteLine(s);
			}
		}
	}
}
```

#### Example client
```javascript
<script type="text/javascript">
var socket;
window.onload = function() {
  connect();
}

function connect() {
	socket = new WebSocket("ws://127.0.0.1:8001/example");
	
	socket.onopen = function(){
		socket.send("hello world.");
	}
	socket.onmessage = function (msg) {
		alert(msg.data);
	}
}
</script>
```

[1]: http://en.wikipedia.org/wiki/WebSocket
[2]: http://www.microsoft.com/en-us/download/details.aspx?id=9983
[3]: http://tools.ietf.org/html/rfc6455