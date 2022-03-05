using UnityEngine;

public abstract class StaticMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T Instance;

    public static T Get(bool DontDestroy = true)
    {
        if(Instance == null) 
        {
            GameObject go = new GameObject(typeof(T).Name+"_Static");
            if (DontDestroy)
            {
                DontDestroyOnLoad(go);
            }
            Instance = go.AddComponent<T>();
        }
        return Instance;
    }
}
