using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

class ClientInstance
{
    public TcpClient TCPClient { get; private set; }
    public String RemoteEndpoint { get; private set; }
    //public bool Authed { get; private set; }
    //public String Name { get; private set; }
    public String YPos { get; private set; }
    public int pnum;

    public ClientInstance(TcpClient tcp)
    {
        this.TCPClient = tcp;
        this.RemoteEndpoint = tcp.Client.RemoteEndPoint.ToString();
        this.YPos = "20";
        this.pnum = 0;
        //this.Name = "Unknown";
    }

    /*public void setName(string name) {
        this.Name = name;
        this.Authed = true;
    }*/

    public void setY(string y)
    {
        this.YPos = y;
    }
}
