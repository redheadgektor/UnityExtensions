using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool
{
    private static GameObject PoolContainer;
    private List<AudioSource> pool;

    public AudioSourcePool(string name = "",Transform parent = null, int capacity = 8)
    {
        pool = new List<AudioSource>(capacity);
        if (!PoolContainer)
        {
            PoolContainer = new GameObject(name);
            PoolContainer.transform.SetParent(parent);
            if (parent == null)
            {
                GameObject.DontDestroyOnLoad(PoolContainer);
            }
        }
        for (int i = 0; i < capacity; i++)
        {
            GameObject go = new GameObject("Pooled_" + pool.Count);
            go.transform.SetParent(PoolContainer.transform);
            AudioSource _as = go.AddComponent<AudioSource>();
            pool.Add(_as);
        }
    }

    ~AudioSourcePool()
    {
        Destroy();
    }

    public void Destroy()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
            {
                GameObject.Destroy(pool[i].gameObject);
            }
        }

        pool.Clear();

        if (PoolContainer)
        {
            GameObject.Destroy(PoolContainer);
        }
    }

    public AudioSource Get()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            if(pool[i] != null && !pool[i].isPlaying)
            {
                return pool[i];
            }
        }

        return null;
    }

    public void Play(AudioClip clip, float volume = 1, float pitch = 1)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource s = Get();
        s.clip = clip;
        s.volume = volume;
        s.pitch = pitch;
        s.Play();
    }
}
