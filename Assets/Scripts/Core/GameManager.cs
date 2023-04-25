using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{

    [SerializeField]
    private GameObject playerPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = Constants.TICKS_PER_SEC;

        NetworkManager.Singleton.Run(100, 26950);
    }

    private void OnApplicationQuit()
    {
        NetworkManager.Singleton.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
