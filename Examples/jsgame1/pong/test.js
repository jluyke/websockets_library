var socket;
var mouseX, mouseY;
var paddle, paddleMove, paddleSpeed, paddleHeight;
var canvasW, canvasH;
var p1score, p2score;
var localXPos, otherPlayerX, otherPlayerY;
var pnum;
var ball;

window.onload = function() {
	canvas = document.getElementById("canvasid");
	rect = canvas.getContext('2d');
	canvas.style.position = "fixed";
	initVars();
	connect();
}

window.onunload = function() {
	var bytes = new Uint8Array(1);
	bytes[0] = 255;
	socket.send(bytes.buffer); //sends d/c
	socket.close();
}
		
window.onresize = function() {
	//canvas.style.position = "fixed";
	//canvas.style.top = 100;
}

window.onkeydown = function(e) {
	switch (e.keyCode) {
		case 38:
			paddleMove = -paddleSpeed;
			break;
		case 40:
			paddleMove = paddleSpeed;
			break;
		default:
			break;
	}
}

window.onkeyup = function(e) {
	if (paddleMove == paddleSpeed && e.keyCode == 40) {
		paddleMove = 0;
	} else if (paddleMove == -paddleSpeed && e.keyCode == 38) {
		paddleMove = 0;
	}
}

window.onmousemove = function(e) {
    if(e.offsetX) {
        mouseX = e.offsetX;
        mouseY = e.offsetY;
    } else if(e.layerX) {
        mouseX = e.layerX;
        mouseY = e.layerY;
    }
}

function connect() {
	socket = new WebSocket("ws://127.0.0.1:8001/session");
	
	socket.onopen = function(){
		canvas.width = canvasW;
		canvas.height = canvasH;
		setInterval(draw, 1);
   	}
    socket.onclose = function() {
    	canvas.width = 0;
		canvas.height = 0;
    }
    socket.onmessage = function (msg) {
    	addMsgArray(msg.data);
	}
}

function initVars() {
	paddle = 20;
	paddleMove = 0;
	paddleSpeed = 2;
	paddleHeight = 60;
	canvasW = 800;
	canvasH = 400;
	ball = new Array();
}

function addMsgArray(msg) {
	var msgArray = msg.split(" ");
	for (var i = 0; i < msgArray.length; i++) {
		var split = msgArray[i].split("=");
		switch (split[0]) {
			case "y":
				otherPlayerY = parseInt(split[1]);
				break;
			case "num":
				pnum = split[1];
				if (pnum == "1") {
					localXPos = 20;
					otherPlayerX = 770;
				} else if (pnum == "2") {
					localXPos = 770;
					otherPlayerX = 20;
				}
				
				break;
			case "bX":
				ball[0] = parseInt(split[1]);
				break;
			case "bY":
				ball[1] = parseInt(split[1]);
				break;
			default:
				break;
		}	
	}
}

function update() {
	if (paddle > 5 && paddle < canvasH - paddleHeight - 5)
		paddle += paddleMove;
	else if (paddleMove == paddleSpeed && paddle <= 5 || paddleMove == -paddleSpeed && paddle >= canvasH - paddleHeight - 5)
		paddle += paddleMove;
}

function draw() {
	rect.clearRect(0, 0, canvas.width, canvas.height);
	update();
	//local paddle
	rect.fillRect(localXPos,paddle,10,paddleHeight);
	rect.fillStyle = "#FFFFFF";
	//paddle2
	rect.fillRect(otherPlayerX,otherPlayerY,10,60);
	rect.fillStyle = "#FFFFFF";
	//rect.fillRect()
	//rect.fillText(otherPlayerY, 200, 40);
	rect.fillRect(ball[0], ball[1], 10, 10);
	socket.send("y=" + paddle);
}
