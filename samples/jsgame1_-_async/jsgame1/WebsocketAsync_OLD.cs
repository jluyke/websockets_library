using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class WebsocketAsync //This uses TcpClients instead of Sockets. This is for this example only.
{
    private TcpListener server;

    /// <summary>
    /// Sets up and starts server, IP bind and port.
    /// </summary>
    /// <param name="serverIP">IPAddress.Any can be used.</param>
    /// <param name="port"></param>
    public WebsocketAsync(IPAddress serverIP, int port)
    {
        server = new TcpListener(serverIP, port);
        server.AllowNatTraversal(true);
        server.Start();
    }
    /// <summary>
    /// Accepts and pairs server and client with handshake, returns client socket. If no pending clients, returns null.
    /// </summary>
    /// <returns></returns>
    public async Task<TcpClient> AcceptPendingClient()
    {
        //Socket newclient = null;
        TcpClient newclient = null;
        if (server.Pending()) {
            newclient = await server.AcceptTcpClientAsync();
            Console.WriteLine("[{0}] requested handshake.", newclient.Client.RemoteEndPoint);
            byte[] buffer = new byte[255];
            //int len = newclient.Receive(buffer); //if this is fragmented, full string will not be received and crashes
            int len = await newclient.GetStream().ReadAsync(buffer, 0, buffer.Length);
            //Console.WriteLine("debug: handshake request length: " + len); //full msg length is 220-240
            if (len >= 200) {
                string reply = HandshakeResponse(buffer);
                await newclient.GetStream().WriteAsync(UTF8Encoding.UTF8.GetBytes(reply), 0, UTF8Encoding.UTF8.GetBytes(reply).Length);
                Console.WriteLine("[{0}] handshake matched.", newclient.Client.RemoteEndPoint);
            }
        }
        return newclient;
    }
    /// <summary>
    /// Sends data as string to specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public async Task Send(TcpClient client, string message)
    {
        byte[] buffer = toSend(message);
        await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }
    /// <summary>
    /// Receives pending data as string from specified socket.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    public async Task<string> Receive(TcpClient client)
    {
        byte[] buffer = new byte[255]; //todo this
            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable) {
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return toReceive(buffer);
            }
            return null;
    }

    private string HandshakeResponse(byte[] buffer)
    {
        string[] lines = UTF8Encoding.UTF8.GetString(buffer).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //for (int i = 0; i < lines.Length; i++)
            //Console.WriteLine(lines[i]);
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

    private string toReceive(byte[] buffer)
    {
        //Console.WriteLine(BitConverter.ToString(buffer, 0, buffer.Length));
        //int msgType = Convert.ToInt32(buffer[0]) - 128;
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
        } else {
            Console.WriteLine("This number is too big."); //if 127, greater than ~65kB
        }
        for (int i = 0; i < packetlength; i++)
            buffer[sIndex + i] ^= mask[i % 4];
        //Console.WriteLine("length: " + packetlength);
        string result = System.Text.UTF8Encoding.UTF8.GetString(buffer, sIndex, packetlength);
        return result;
    }

    private byte[] toSend(string msg)
    {
        byte[] frame;
        byte[] b = UTF8Encoding.UTF8.GetBytes(msg);
        if (msg.Length < 126) {
            frame = new byte[2 + msg.Length];
            frame[1] = Convert.ToByte(msg.Length);
            for (int i = 2, j = 0; j < b.Length; i++, j++)
                frame[i] = b[j];
        } else {
            frame = new byte[4 + msg.Length];
            frame[1] = 0x7E;
            double d = Math.Truncate((double)msg.Length / 255);
            frame[2] = Convert.ToByte(d);
            frame[3] = Convert.ToByte(msg.Length - d * 255 - d);
            for (int i = 4, j = 0; j < b.Length; i++, j++)
                frame[i] = b[j];
        }
        frame[0] = 0x81;
        //Console.WriteLine("sending: " + BitConverter.ToString(frame, 0, frame.Length));
        return frame;
    }
}

