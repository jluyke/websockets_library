using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

class Websocket
{
    private TcpListener server;

    /// <summary>
    /// Configures and starts server, IP bind and port.
    /// </summary>
    /// <param name="serverIP">IPAddress.Any may be used.</param>
    /// <param name="port"></param>
    public Websocket(IPAddress serverIP, int port)
    {
        server = new TcpListener(serverIP, port);
        server.AllowNatTraversal(true);
        server.Start();
    }
    /// <summary>
    /// Accepts and pairs server and client with handshake, returns client socket. If no pending clients, returns null.
    /// </summary>
    /// <returns>Accepted socket</returns>
    public Socket AcceptSocket()
    {
        Socket newclient = null;
        if (server.Pending()) {
            newclient = server.AcceptSocket();
            Console.WriteLine("[{0}] requested handshake.", newclient.RemoteEndPoint);
            byte[] buffer = new byte[260];
            int len = newclient.Receive(buffer); //if this is fragmented, full string will not be received and crashes
            //Console.WriteLine("debug: handshake request length: " + len); //full msg length is 220-240
            if (len >= 200) {
                string reply = HandshakeResponse(buffer);
                newclient.Send(UTF8Encoding.UTF8.GetBytes(reply));
                Console.WriteLine("[{0}] handshake matched.", newclient.RemoteEndPoint);
            }
        }
        return newclient;
    }
    /// <summary>
    /// Sends data as string to specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public void SendString(Socket socket, string message)
    {
        byte[] buffer = UTF8Encoding.UTF8.GetBytes(message);
        buffer = toSend(buffer);
        socket.Send(buffer);
    }
    /// <summary>
    /// Sends data as byte array to specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public void Send(Socket socket, byte[] buffer)
    {
        buffer = toSend(buffer);
        socket.Send(buffer);
    }
    /// <summary>
    /// Receives pending data (if any) as string from specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    public string ReceiveString(Socket socket)
    {
        byte[] buffer;
        string result = "";
        if (socket.Available > 0) {
            buffer = new byte[65535];
            socket.Receive(buffer);
            buffer = toReceive(buffer);
            result = System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
        return result;
    }
    /// <summary>
    /// Receives pending data (if any) as bytes from specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    public byte[] Receive(Socket socket)
    {
        byte[] buffer = { 0 };
        if (socket.Available > 0) {
            buffer = new byte[65535];
            socket.Receive(buffer);
            buffer = toReceive(buffer);
        }
        return buffer;
    }

    private string HandshakeResponse(byte[] buffer)
    {
        string[] lines = UTF8Encoding.UTF8.GetString(buffer).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        string oldkey = lines[5].Split(' ')[1];
        byte[] data = UTF8Encoding.UTF8.GetBytes(oldkey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
        SHA1 sha = new SHA1CryptoServiceProvider();
        byte[] hash = sha.ComputeHash(data);
        string newkey = System.Convert.ToBase64String(hash);
        //Console.WriteLine(oldkey + " handshake with " + newkey);
        string handshake =
            "HTTP/1.1 101 Switching Protocols" + Environment.NewLine +
            "Upgrade: websocket" + Environment.NewLine +
            "Connection: Upgrade" + Environment.NewLine +
            "Sec-WebSocket-Accept: " + newkey + Environment.NewLine + Environment.NewLine;
        return handshake;
    }

    private byte[] toReceive(byte[] buffer)
    {
        int packetlength = Convert.ToInt32(buffer[1] - 128);
        int sIndex = 0;
        byte[] mask = new byte[4];
        if (packetlength < 126) {
            sIndex = 6;
            mask[0] = buffer[2];
            mask[1] = buffer[3];
            mask[2] = buffer[4];
            mask[3] = buffer[5];
        } else if (packetlength == 126) {
            sIndex = 8;
            packetlength = Convert.ToUInt16(buffer[2]) * 255 + Convert.ToUInt16(buffer[2]) + Convert.ToUInt16(buffer[3]);
            mask[0] = buffer[4];
            mask[1] = buffer[5];
            mask[2] = buffer[6];
            mask[3] = buffer[7];
        } else if (packetlength == 127) {
            Console.WriteLine("This number is too big."); //if 127, greater than ~65kB
        }
        for (int i = 0; i < packetlength; i++)
            buffer[sIndex + i] ^= mask[i % 4];
        byte[] bufferResult = new byte[packetlength];
        Array.Copy(buffer, sIndex, bufferResult, 0, packetlength);
        return bufferResult;
    }

    private byte[] toSend(byte[] b)
    {
        byte[] frame;
        if (b.Length < 126) {
            frame = new byte[2 + b.Length];
            frame[1] = Convert.ToByte(b.Length);
            for (int i = 0; i < b.Length; i++)
                frame[i+2] = b[i];
        } else {
            frame = new byte[4 + b.Length];
            frame[1] = 0x7E;
            double d = Math.Truncate((double)b.Length / 255);
            frame[2] = Convert.ToByte(d);
            frame[3] = Convert.ToByte(b.Length - d * 255 - d);
            for (int i = 4; i < b.Length; i++)
                frame[i+4] = b[i];
        }
        frame[0] = 0x81;
        //Console.WriteLine("sending: " + BitConverter.ToString(frame, 0, frame.Length));
        return frame;
    }
}

/*
   Copyright [2012] [Jordan Luyke]

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/