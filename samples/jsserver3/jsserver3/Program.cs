using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

                Thread.Sleep(20);
            }
        }

        static void receiveData()
        {
            if (clientlist.Count > 0) {
                for (int i = 0; i < clientlist.Count; i++) {
                    if (clientlist[i].sClient.Available > 0) {
                        //todo: get everything received esp last received to improve performance
                        try {
                            string msg = websocket.ReceiveString(clientlist[i].sClient);
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
                            //Console.WriteLine(userinfo);
                        } catch {
                            //Console.WriteLine("Sending failed, removing " + clientlist[i].Name + " at " + clientlist[i].RemoteEndpoint);
                            clientlist.RemoveAt(i);
                            //removeList.Add(i);
                            //i++;
                            //j = 0;
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
