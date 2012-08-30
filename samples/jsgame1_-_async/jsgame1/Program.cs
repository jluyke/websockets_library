using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace jsgame1
{
    class Program
    {
        static WebsocketAsync websocket;
        static List<ClientInstance> clientlist;
        static PongCalcs pong;

        static void Main(string[] args)
        {
            websocket = new WebsocketAsync(IPAddress.Any, 8001);
            clientlist = new List<ClientInstance>();
            pong = new PongCalcs(websocket);
            Console.Title = "jsgame1";

            while (true) {
                accept();
                receive();
                pong.calcs(ref clientlist);
                Thread.Sleep(10);
            }
        }

        static void accept()
        {
            TcpClient client = websocket.AcceptPendingClient().Result;

            if (client != null && clientlist.Count <= 2) {
                clientlist.Add(new ClientInstance(client));
                if (clientlist.Count >= 2) {
                    if (clientlist[0].pnum == 1) {
                        websocket.Send(clientlist[clientlist.Count - 1].TCPClient, "num=2");
                        clientlist[clientlist.Count - 1].pnum = 2;
                    } else if (clientlist[0].pnum == 2) {
                        websocket.Send(clientlist[clientlist.Count - 1].TCPClient, "num=1");
                        clientlist[clientlist.Count - 1].pnum = 1;
                    }
                } else {
                    websocket.Send(clientlist[clientlist.Count - 1].TCPClient, "num=1");
                    clientlist[clientlist.Count - 1].pnum = 1;
                }
            }
        }

        static void receive()
        {
            if (clientlist.Count > 0) {
                for (int i = 0; i < clientlist.Count; i++) {
                    try {
                        string msg = websocket.Receive(clientlist[i].TCPClient).Result;
                        if (msg != null) {
                            string[] msglist = msg.Split(' ');
                            //Console.WriteLine(msglist[0]);
                            foreach (string s in msglist) {
                                string[] split = s.Split('=');
                                switch (split[0]) {
                                    case "y":
                                        //Console.WriteLine(i + ": " + split[1]);
                                        clientlist[i].setY(split[1]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            for (int j = 0; j < clientlist.Count; j++) {
                                if (j != i)
                                    websocket.Send(clientlist[j].TCPClient, "y=" + clientlist[i].YPos);
                            }
                        }
                    } catch {
                        Console.WriteLine("Error on receive, removing client " + i + " on " + clientlist[i].RemoteEndpoint);
                        clientlist.RemoveAt(i);
                    }
                }
            }
        }
    }
}
