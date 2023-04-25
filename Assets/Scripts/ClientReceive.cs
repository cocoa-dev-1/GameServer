using UnityEngine;
using System;

public class ClientReceive
{
    #region TCP receive

    public static void WelcomeReceived(int clientId, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{NetworkManager.Singleton.clients[clientId].tcp.socket.Client.RemoteEndPoint} connected successfully. clientId : {clientId}");

        if (clientId != clientIdCheck)
        {
            Debug.Log($"Player '{username}' ({clientId}) has assumed the wrong client ID ({clientIdCheck})");
        }

        NetworkManager.Singleton.clients[clientId].SendIntoGame(username);
    }

    #endregion

    #region UDP receive

    public static void PlayerMovement(int clientId, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        if (NetworkManager.Singleton.clients.TryGetValue(clientId, out ClientHandle client))
        {
            if (client.player == null) return;

            client.player.SetInput(inputs, rotation);
        }
    }

    #endregion
}

