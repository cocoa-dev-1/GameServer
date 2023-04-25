using System;
using UnityEngine;

public class ClientSend
{
    #region Send Data

    private static void SendTCPData(int targetClient, Packet packet)
    {
        packet.InsertLength();
        NetworkManager.Singleton.clients[targetClient].tcp.SendData(packet);
    }

    private static void SendTCPDataAll(Packet packet)
    {
        packet.InsertLength();

        for (int i = 1; i < NetworkManager.Singleton.MaxPlayers; i++)
        {
            NetworkManager.Singleton.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataAll(int myClient, Packet packet)
    {
        packet.InsertLength();

        for (int i = 1; i < NetworkManager.Singleton.MaxPlayers; i++)
        {
            if (i == myClient) continue;
            NetworkManager.Singleton.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendUDPDataAll(Packet packet)
    {
        packet.InsertLength();

        for (int i = 1; i < NetworkManager.Singleton.MaxPlayers; i++)
        {
            NetworkManager.Singleton.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataAll(int myClient, Packet packet)
    {
        packet.InsertLength();

        for (int i = 1; i < NetworkManager.Singleton.MaxPlayers; i++)
        {
            if (i == myClient) continue;
            NetworkManager.Singleton.clients[i].udp.SendData(packet);
        }
    }

    #endregion

    #region TCP Packets

    public static void Welcome(int targetClient, string msg)
    {
        Debug.Log($"sending welcome to {targetClient}");
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(targetClient);
            packet.Write(msg);

            SendTCPData(targetClient, packet);
        }
    }

    public static void SpawnPlayer(int targetClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(targetClient, packet);
        }
    }

    #endregion

    #region UDP Packets

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataAll(player.id, packet);
        }
    }

    #endregion
}

