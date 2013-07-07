using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace jsserver3
{
    class Program
    {
        static Websocket websocket;
        static List<ClientInstance> clientlist = new List<ClientInstance>();

        static void Main(string[] args)
        {
            Console.Title = "jsserver3";
            websocket = new Websocket(IPAddress.Any, 8001);

            while (true) {
                Socket client = websocket.AcceptSocket();
                if (client != null)
                    clientlist.Add(new ClientInstance(client));

                receiveData();

                sendData();
            }
        }

        static void receiveData()
        {
            if (clientlist.Count > 0) {
                for (int i = 0; i < clientlist.Count; i++) {
                    if (clientlist[i].sClient.Available > 0) {
                        try {
                            string msg = websocket.ReceiveString(clientlist[i].sClient);
                            string fin = msg.Substring(0, 1);
                            string opcode = msg.Substring(1, 1);
                            msg = msg.Substring(2);
                            //If fin = 1 (final) and opcode = 1 (text frame)
                            if (fin == "1" && opcode == "1")
                                setClientData(i, msg);
                        } catch {
                            Console.WriteLine("[{0}] corrupted data has been received. Did client close session?");
                        }
                    }
                }
            }
        }

        static void sendData()
        {
            List<int> removeList = new List<int>();
            for (int i = 0; i < clientlist.Count; i++) {
                for (int j = 0; j < clientlist.Count; j++) {
                    if (i != j) {
                        try {
                            string userinfo = "i=" + clientlist[j].RemoteEndpoint.Split(':')[1] + " n=" + clientlist[j].Name + " x=" + clientlist[j].X + " y=" + clientlist[j].Y + " p=" + clientlist[j].Pressing + " r=" + clientlist[j].Reset;
                            websocket.SendString(clientlist[i].sClient, userinfo);
                        } catch {
                            //Console.WriteLine("Sending failed, removing " + clientlist[i].Name + " at " + clientlist[i].RemoteEndpoint);
                            clientlist.RemoveAt(i);
                        }
                    }
                }
            }
        }

        static void setClientData(int clientNum, string msg)
        {
            string[] msglist = msg.Split(' ');
            foreach (string s in msglist) {
                string[] split = s.Split('=');
                switch (split[0]) {
                    case "name":
                        Console.WriteLine("[{0}] is now known as: [{1}]", clientlist[clientNum].RemoteEndpoint, split[1]);
                        clientlist[clientNum].setName(split[1]);
                        break;
                    case "x":
                        clientlist[clientNum].setX(split[1]);
                        break;
                    case "y":
                        clientlist[clientNum].setY(split[1]);
                        break;
                    case "p":
                        clientlist[clientNum].setP(split[1]);
                        break;
                    case "r":
                        clientlist[clientNum].setR(split[1]);
                        break;
                    case "ff":
                        Console.WriteLine(clientlist[clientNum].Name + " has closed the session.");
                        clientlist.RemoveAt(clientNum);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
