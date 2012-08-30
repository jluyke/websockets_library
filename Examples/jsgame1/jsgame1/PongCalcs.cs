using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

class PongCalcs
{
    JSWebsocket websocket;
    private double[] ball = new double[4];
    private double ballSpeed = 0.0007;
    private Random r = new Random();

    public PongCalcs(JSWebsocket w)
    {
        this.websocket = w;
        resetBall();
    }

    public void calcs(ref List<ClientInstance> clientlist)
    {
        if (clientlist.Count == 2) {
            for (int i = 0; i < clientlist.Count; i++) {
                try {
                    if (ball[0] < 30) {
                        if (clientlist[0].pnum == 1)
                            if (ball[1] + 10 > Convert.ToInt32(clientlist[0].YPos) && ball[1] < Convert.ToInt32(clientlist[0].YPos) + 60)
                                ball[2] *= -1;
                            else
                                resetBall();
                        else if (clientlist[1].pnum == 1)
                            if (ball[1] + 10 > Convert.ToInt32(clientlist[1].YPos) && ball[1] < Convert.ToInt32(clientlist[1].YPos) + 60)
                                ball[2] *= -1;
                            else
                                resetBall();
                    } else if (ball[0] > 760) {
                        if (clientlist[0].pnum == 2)
                            if (ball[1] + 10 > Convert.ToInt32(clientlist[0].YPos) && ball[1] < Convert.ToInt32(clientlist[0].YPos) + 60)
                                ball[2] *= -1;
                            else
                                resetBall();
                        else if (clientlist[1].pnum == 2)
                            if (ball[1] + 10 > Convert.ToInt32(clientlist[1].YPos) && ball[1] < Convert.ToInt32(clientlist[1].YPos) + 60)
                                ball[2] *= -1;
                            else
                                resetBall();
                    }
                    if (ball[1] < 0 || ball[1] > 390) {
                        ball[3] *= -1;
                    }
                    ball[0] += ball[2];
                    ball[1] += ball[3];
                    ball[2] = (Math.Abs(ball[2]) + ballSpeed) * Math.Sign(ball[2]);
                    ball[3] = (Math.Abs(ball[3]) + ballSpeed) * Math.Sign(ball[3]);


                    websocket.Send(clientlist[i].TCPClient, "bX=" + ball[0] + " bY=" + ball[1]);
                } catch {
                    clientlist.RemoveAt(i);
                    //websocket.Send(clientlist[clientlist.Count - 1].TCPClient, "num=1");
                }
            }
        }
    }

    private void resetBall()
    {
        ball[0] = 700;
        ball[1] = r.Next(100, 300);
        ball[2] = -1;
        ball[3] = 1;
    }

}

