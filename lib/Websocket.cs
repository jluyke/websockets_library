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
    /// <param name="serverIP"></param>
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
            byte[] buffer = new byte[512];
            int len = newclient.Receive(buffer); //must receive full handshake all at once
            //Console.WriteLine("debug: handshake request length: " + len);
            if (len >= 100) {
                string reply = HandshakeResponse(buffer);
                if (reply.Split(' ')[0] != "Handshake_Failed:") {
                    newclient.Send(UTF8Encoding.UTF8.GetBytes(reply));
                    Console.WriteLine("[{0}] handshake matched.", newclient.RemoteEndPoint);
                } else {
                    Console.WriteLine(reply);
                    newclient = null;
                }
            }
        }
        return newclient;
    }
    /// <summary>
    /// Sends data as byte array to specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public void Send(Socket socket, byte[] buffer)
    {
        buffer = parseSend(buffer);
        socket.Send(buffer);
    }
    /// <summary>
    /// Sends data as string to specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public void SendString(Socket socket, string message)
    {
        byte[] buffer = UTF8Encoding.UTF8.GetBytes(message);
        Send(socket, buffer);
    }
    /// <summary>
    /// Receives pending data as bytes from specified socket. If no data, returns null.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns>If no data, returns null.</returns>
    public byte[] Receive(Socket socket)
    {
        try {
            byte[] buffer = new byte[11];
            if (socket.Available > 0) {
                socket.Receive(buffer, 2, SocketFlags.None);
                int msgtype = buffer[0];
                int fin = msgtype - 128 > 0 ? 1 : 0;
                int opcode = fin == 1 ? msgtype - 128 : msgtype;
                int msglength = 0;
                int sindex = 0;
                if (buffer[1] - 128 < 126) {
                    sindex = 2;
                    msglength = buffer[1] - 128;
                    buffer = new byte[msglength + 6];
                } else if (buffer[1] - 128 == 126) {
                    sindex = 4;
                    socket.Receive(buffer, 2, 2, SocketFlags.None);
                    msglength = buffer[2] * 255 + buffer[2] + buffer[3];
                    buffer = new byte[msglength + 8];
                } else if (buffer[1] - 128 == 127) {
                    //too long
                }
                buffer[0] = Convert.ToByte(0x00); //placeholders
                buffer[1] = Convert.ToByte(0x00);
                //socket.Receive(buffer, sindex, buffer.Length - sindex, SocketFlags.None); //old
                for (int len = msglength, savbl = 0, i = sindex; len > 0; len -= savbl, i += savbl) {
                    savbl = socket.Available > buffer.Length - sindex ? buffer.Length - sindex : socket.Available;
                    socket.Receive(buffer, i, savbl, SocketFlags.None); //attempts to put packets of same msg together
                }
                buffer = parseReceive(buffer, msglength);
                byte[] subBuffer = new byte[msglength + 2]; //make room for fin and opcode
                Array.Copy(buffer, buffer.Length - msglength, subBuffer, 2, msglength);
                subBuffer[0] = Convert.ToByte(fin);
                subBuffer[1] = Convert.ToByte(opcode);
                return subBuffer;
            } else {
                return null;
            }
        } catch (Exception e) {
            Console.WriteLine("Receive method failed: " + e.ToString());
            return null;
        }
    }
    /// <summary>
    /// Receives pending data as string from specified socket. If no data, returns null.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    public string ReceiveString(Socket socket)
    {
        try {
            byte[] received = Receive(socket);
            return received[0].ToString("X") + received[1].ToString("X") + System.Text.UTF8Encoding.UTF8.GetString(received, 2, received.Length - 2);
        } catch {
            return null;
        }
    }

    private string HandshakeResponse(byte[] buffer)
    {
        try {
            string[] lines = UTF8Encoding.UTF8.GetString(buffer).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            //Console.WriteLine(UTF8Encoding.UTF8.GetString(buffer));
            //Console.WriteLine("handshake: " + BitConverter.ToString(buffer, 0, buffer.Length));
            string oldkey = "";
            for (int i = 0, key = 0, version = 0; i < lines.Length; i++) {
                string[] split = lines[i].Split(' ');
                if (split[0] == "Sec-WebSocket-Key:") {
                    oldkey = lines[i].Split(' ')[1];
                    key = 1;
                } else if (split[0] == "Sec-WebSocket-Version:" && split[1] == "13") {
                    version = 1;
                }
                if (key == 1 && version == 1)
                    break;
                if (i == lines.Length - 1 || i > 20)
                    return "Handshake_Failed: Opening handshake does not contain key or correct WS version";
            }
            byte[] data = UTF8Encoding.UTF8.GetBytes(oldkey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(data);
            string newkey = System.Convert.ToBase64String(hash);
            //Console.WriteLine(oldkey[1] + " handshake with " + newkey);
            string handshake =
                "HTTP/1.1 101 Switching Protocols" + Environment.NewLine +
                "Upgrade: websocket" + Environment.NewLine +
                "Connection: Upgrade" + Environment.NewLine +
                "Sec-WebSocket-Accept: " + newkey + Environment.NewLine + Environment.NewLine;
            return handshake;
        } catch {
            return "Handshake_Failed: Parsing opening handshake failed";
        }
    }

    private byte[] parseReceive(byte[] buffer, int msglength)
    {
        try {
            int startIndex = 0;
            byte[] mask = new byte[4];
            if (msglength < 126) {
                startIndex = 6;
                mask[0] = buffer[2];
                mask[1] = buffer[3];
                mask[2] = buffer[4];
                mask[3] = buffer[5];
            } else if (msglength >= 126 && msglength <= 65536) {
                startIndex = 8;
                mask[0] = buffer[4];
                mask[1] = buffer[5];
                mask[2] = buffer[6];
                mask[3] = buffer[7];
            } else if (msglength > 65536) {
                Console.WriteLine("This number is too big."); //greater than ~65kB
            }
            for (int i = 0; i < msglength; i++)
                buffer[startIndex + i] ^= mask[i % 4];
            return buffer;
        } catch {
            Console.WriteLine("ERROR: parse receive failed******");
            Console.WriteLine(BitConverter.ToString(buffer, 0, buffer.Length));
            Console.WriteLine(UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            Console.WriteLine("*********************************");
            return buffer;
        }
    }

    private byte[] parseSend(byte[] b)
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