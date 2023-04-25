using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;

public class NetworkManager : SingletonBehaviour<NetworkManager>
{
    public int MaxPlayers { get; private set; }
    public int Port { get; private set; }

    public Dictionary<int, ClientHandle> clients;
    public delegate void PacketHandler(int fromClient, Packet packet);
    public Dictionary<int, PacketHandler> packetHandlers;

    public TcpListener tcpListener;
    public UdpClient udpListener;

    private void Awake()
    {
        clients = new Dictionary<int, ClientHandle>();
    }

    public void Run(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        InitializeClients();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(new AsyncCallback(UDPReceiveCallback), null);

        Debug.Log($"Server started on port : {Port}");
    }

    private void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient tcpClient = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incomming connection from {tcpClient.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(tcpClient);
                return;
            }
        }

        Debug.Log($"Server are full. client : {tcpClient.Client.RemoteEndPoint}");
    }

    private void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref endPoint);
            udpListener.BeginReceive(new AsyncCallback(UDPReceiveCallback), null);

            if (data.Length < 4) return;

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0) return;

                if (clients[clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[clientId].udp.Connect(endPoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() != endPoint.ToString()) return;

                //Debug.Log($"udp data from client {clientId}");

                clients[clientId].udp.HandleData(packet);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving UDP data : {e}");
        }
    }

    public void SendUDPData(IPEndPoint endPoint, Packet packet)
    {
        try
        {
            if (endPoint == null) return;

            udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        }
        catch (Exception e)
        {
            Debug.Log($"Error sending data to {endPoint} via UDP : {e}");
        }
    }

    private void InitializeClients()
    {
        for(int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new ClientHandle(i));
        }

        Debug.Log("Clients initialized!");

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ClientReceive.WelcomeReceived },
            { (int)ClientPackets.playerMovement, ClientReceive.PlayerMovement }
        };

        Debug.Log("Packet handlers initialized!");
    }

    public void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
