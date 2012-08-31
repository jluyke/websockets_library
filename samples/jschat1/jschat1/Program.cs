using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace jschat1
{
    class Program
    {
        static Websocket websocket;
        static List<ClientInstance> clientlist;

        static void Main(string[] args)
        {
            Console.Title = "jschat1";
            websocket = new Websocket(IPAddress.Any, 8001);
            clientlist = new List<ClientInstance>();

            while (true) {
                accept();
                receive();
                Thread.Sleep(50);
            }
        }

        static void accept()
        {
            Socket client = websocket.AcceptSocket();
            if (client != null)
                clientlist.Add(new ClientInstance(client));
        }

        static void receive()
        {
            if (clientlist.Count > 0) {
                for (int i = 0; i < clientlist.Count; i++) {
                    if (clientlist[i].sClient.Available > 0) {
                        try {
                            string msg = websocket.ReceiveString(clientlist[i].sClient);
                            if (BitConverter.ToString(UTF8Encoding.UTF8.GetBytes(msg)) == "EF-BF-BD") { //hex: 255, d/c request
                                Console.WriteLine("[{0}] closed session.", clientlist[i].Name);
                                if (clientlist[i].Authed) {
                                    send(i, clientlist[i].Name + " has disconnected.");
                                    clientlist[i].sClient.Close();
                                    clientlist.RemoveAt(i);
                                } else {
                                    clientlist[i].sClient.Close();
                                    clientlist.RemoveAt(i);
                                }
                            } else {
                                if (clientlist[i].Authed) {
                                    Console.WriteLine("[{0}]: {1}", clientlist[i].Name, msg);
                                    send(i, clientlist[i].Name + ": " + msg);
                                } else {
                                    clientlist[i].setName(msg);
                                    Console.WriteLine("[{0}] authenticated and changed name to [{1}]", clientlist[i].RemoteEndpoint, clientlist[i].Name);
                                    send(i, clientlist[i].Name + " has connected.");
                                }
                            }
                        } catch (Exception e) {
                            //Console.WriteLine("[{0}] corrupted data has been received. Did client close session?", clientlist[i].Name);
                            Console.WriteLine("Fail in receive method; exception: " + e.ToString());
                        }
                    }
                }
            }
        }

        static void send(int index, string msg)
        {
            List<int> removeList = new List<int>();
            for (int i = 0; i < clientlist.Count; i++) {
                if (i != index) {
                    try {
                        websocket.SendString(clientlist[i].sClient, msg);
                        //Console.WriteLine(userinfo);
                    } catch {
                        Console.WriteLine("Sending failed, removing " + clientlist[i].Name + " at " + clientlist[i].RemoteEndpoint);
                        removeList.Add(i);
                    }
                }
            }
            //Removing
            for (int i = 0; i < removeList.Count; i++)
                clientlist.RemoveAt(i);
            for (int i = 0; i < removeList.Count; i++)
                if (clientlist[i].Authed)
                    send(i, clientlist[i].Name + " has timed out.");
        }
    }
}
