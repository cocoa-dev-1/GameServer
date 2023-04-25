using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Singleton
    {
        get
        {
            if (instance == null)
            {
                try
                {
                    instance = FindObjectOfType<T>();
                }
                catch (Exception e)
                {
                    Debug.Log($"Error getting {typeof(T)} Singleton. error: {e}");
                }
            }

            return instance;
        }
    }
}
