#### About
A wip library for [WebSocket][1] servers, written in .net C#. Currently there is only support for sending and receiving strings synchronously. 
All sample websocket clients, written in javascript, can be found in their respective sample folders.

To use the Async version for pong sample, Visual Studio 10 is required along with [Async CTP][2], or Visual Studio 11.

This library conforms to the [hybi 17 protocol][3].

#### Example server
```csharp
static void Main(string[] args)
{
	Websocket websocket = new Websocket(IPAddress.Any, 8001);
	while (true) {
		Socket client = websocket.AcceptPendingRequest();
		if (client != null) {
			websocket.Send(client, "hello to you.");
			if (client.Available > 0) {
				string s = websocket.Receive(client);
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
	socket = new WebSocket("ws://127.0.0.1:8001/session");
	
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
[3]: http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-17