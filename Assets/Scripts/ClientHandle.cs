using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle
{
    public static int dataBufferSize = 4096;

    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;

    public ClientHandle(int id)
    {
        this.id = id;

        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int id)
        {
            socket = null;

            this.id = id;
            stream = null;
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket == null) return;

                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to {id} via TCP : {e}");
            }
        }

        public void Connect(TcpClient socket)
        {
            this.socket = socket;
            this.socket.ReceiveBufferSize = dataBufferSize;
            this.socket.SendBufferSize = dataBufferSize;

            stream = this.socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);

            ClientSend.Welcome(id, "welcome.");
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int packetLength = stream.EndRead(result);
                if (packetLength <= 0)
                {
                    NetworkManager.Singleton.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[packetLength];
                Array.Copy(receiveBuffer, data, packetLength);

                receivedData.Reset(HandleData(data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception e)
            {
                Debug.Log($"Error receiving data from {id} via TCP : {e}");
                NetworkManager.Singleton.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                // If client's received data contains a packet
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    // If packet contains no data
                    return true; // Reset receivedData instance to allow it to be reused
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        NetworkManager.Singleton.packetHandlers[packetId](id, packet); // Call appropriate method to handle the packet
                    }
                });

                packetLength = 0; // Reset packet length
                if (receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains another packet
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true; // Reset receivedData instance to allow it to be reused
            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private readonly int id;

        public UDP(int id)
        {
            endPoint = null;
            this.id = id;
        }

        public void SendData(Packet packet)
        {
            NetworkManager.Singleton.SendUDPData(endPoint, packet);
        }

        public void Connect(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public void HandleData(Packet packetData)
        {
            int packetLength = packetData.ReadInt();
            byte[] data = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    NetworkManager.Singleton.packetHandlers[packetId](id, packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string username)
    {
        player = GameManager.Singleton.InstantiatePlayer();
        player.Initialize(id, username);

        Debug.Log(NetworkManager.Singleton.clients[id].player);
        

        foreach (ClientHandle client in NetworkManager.Singleton.clients.Values)
        {
            if (client.player == null) continue;

            if (client.id != id) ClientSend.SpawnPlayer(id, client.player);

            ClientSend.SpawnPlayer(client.id, player);
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();
    }
}
