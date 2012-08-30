using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

class ClientInstance
{
    public Socket sClient { get; private set; }
    public String RemoteEndpoint { get; private set; }
    public bool Authed { get; private set; }
    public String Name { get; private set; }
    public String X { get; private set; }
    public String Y { get; private set; }
    public String Pressing { get; private set; }
    public String Reset { get; private set; }

    public ClientInstance(Socket cli)
    {
        this.sClient = cli;
        this.RemoteEndpoint = cli.RemoteEndPoint.ToString();
        this.Name = "Unknown";
        this.X = "0";
        this.Y = "0";
        this.Pressing = "0";
        this.Reset = "0";
    }

    public void Authenticate() {
        this.Authed = true;
    }
    public void setName(string name) {
        this.Name = name;
    }

    public void setX(string x)
    {
        this.X = x;
    }

    public void setY(string y)
    {
        this.Y = y;
    }

    public void setP(string p)
    {
        this.Pressing = p;
    }

    public void setR(string r)
    {
        this.Reset = r;
    }
}
