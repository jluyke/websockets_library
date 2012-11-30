#### About
A library for a [WebSocket][1] server, written in .Net C#. Ability to send and receive in both bytes and strings. 
Google Chrome supports the latest websocket standard, example uses include chat or game apps.

#### How to use
```csharp
Websocket(IPAddress, port);				- initialize class, starts server
websocket.AcceptSocket();				- Accepts and pairs websocket client, returns client socket
websocket.Send(Socket, byte[]);			- Sends byte[] to socket
websocket.SendString(Socket, string);	- Sends string to socket
websocket.Receive(Socket);				- Receives pending data as byte[]
websocket.ReceiveString(Socket);		- Receives pending data as string
```

#### Example server
```csharp
static void Main(string[] args)
{
	Websocket websocket = new Websocket(IPAddress.Any, 8001);
	Socket client = websocket.AcceptSocket();
	websocket.SendString(client, "hello to you.");
	string str = websocket.ReceiveString(client);
	Console.WriteLine(str);
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
Sample websocket clients, written in javascript, can be found in their respective sample folders.

#### More info

All message receives will be preceeded by fin then opcode, e.g. "11Hello World" (first&final text frame) or "18" (close connection).
Quoted from the [WebSocket Proposed Standard][3]:
```javscript
[FIN bit, Opcode]
EXAMPLE: For a text message sent as three fragments, the first
fragment would have an opcode of 0x1 and a FIN bit clear (01), the
second fragment would have an opcode of 0x0 and a FIN bit clear (00),
and the third fragment would have an opcode of 0x0 and a FIN bit
that is set (10).
      
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
Read more about the exact data framing structure [here][4].

[1]: http://en.wikipedia.org/wiki/WebSocket
[2]: http://www.microsoft.com/en-us/download/details.aspx?id=9983
[3]: http://tools.ietf.org/html/rfc6455
[4]: http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-17#section-5.2