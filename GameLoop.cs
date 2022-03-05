using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public interface IEarlyUpdate { void EarlyUpdate(); }
public interface IFixedUpdate { void FixedUpdate(); }
public interface IPreFrameUpdate { void PreFrameUpdate(); }
public interface IFrameUpdate { void FrameUpdate(); }
public interface IPreLateUpdate { void PreLateUpdate(); }
public interface IPostLateUpdate { void PostLateUpdate(); }

public class GameLoop
{
    public static void InsertSystem<T>(int index, PlayerLoopSystem.UpdateFunction function, Type playerLoopSystemType)
    {
        PlayerLoopSystem systems = PlayerLoop.GetCurrentPlayerLoop();
        InsertSystem<T>(index, function, ref systems, playerLoopSystemType);
        PlayerLoop.SetPlayerLoop(systems);
    }
    static bool InsertSystem<T>(int index, PlayerLoopSystem.UpdateFunction function, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            var list = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            PlayerLoopSystem sys = new PlayerLoopSystem();
            sys.type = typeof(T);
            sys.updateDelegate = function;
            list.Insert(index, sys);
            playerLoop.subSystemList = list.ToArray();
            return true;
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (InsertSystem<T>(index, function, ref playerLoop.subSystemList[i], playerLoopSystemType))
                    return true;
            }
        }
        return false;
    }

    public static void EjectSystem<T>(int index, Type playerLoopSystemType)
    {
        PlayerLoopSystem systems = PlayerLoop.GetCurrentPlayerLoop();
        EjectSystem<T>(index, ref systems, playerLoopSystemType);
        PlayerLoop.SetPlayerLoop(systems);
    }
    static bool EjectSystem<T>(int index, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            var list = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            if (list[index].type == typeof(T))
            {
                list.RemoveAt(index);
                playerLoop.subSystemList = list.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (EjectSystem<T>(index, ref playerLoop.subSystemList[i], playerLoopSystemType))
                    return true;
            }
        }
        return false;
    }

    public static void AddSystem<T>(PlayerLoopSystem.UpdateFunction function, Type playerLoopSystemType)
    {
        PlayerLoopSystem systems = PlayerLoop.GetCurrentPlayerLoop();
        AddSystem<T>(function, ref systems, playerLoopSystemType);
        PlayerLoop.SetPlayerLoop(systems);
    }
    static bool AddSystem<T>(PlayerLoopSystem.UpdateFunction function, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            var list = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            PlayerLoopSystem sys = new PlayerLoopSystem();
            sys.type = typeof(T);
            sys.updateDelegate = function;
            list.Add(sys);
            playerLoop.subSystemList = list.ToArray();
            return true;
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (AddSystem<T>(function, ref playerLoop.subSystemList[i], playerLoopSystemType))
                    return true;
            }
        }
        return false;
    }

    public static bool RemoveSystem<T>(Type playerLoopSystemType)
    {
        PlayerLoopSystem systems = PlayerLoop.GetCurrentPlayerLoop();
        bool success = RemoveSystem<T>(ref systems, playerLoopSystemType);
        if (success)
        {
            PlayerLoop.SetPlayerLoop(systems);
            return true;
        }
        return false;
    }
    static bool RemoveSystem<T>(ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            var list = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].type == typeof(T))
                {
                    list.RemoveAt(i);
                }
            }
            playerLoop.subSystemList = list.ToArray();
            return true;
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (RemoveSystem<T>(ref playerLoop.subSystemList[i], playerLoopSystemType))
                    return true;
            }
        }
        return false;
    }

    public static bool Initialized { get; private set; } = false;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Initialize()
    {
        if (Initialized)
            return;

        AddSystem<IEarlyUpdate>(EarlyUpdate, typeof(EarlyUpdate));
        InsertSystem<IFixedUpdate>(4, FixedUpdate, typeof(FixedUpdate));
        AddSystem<IPreFrameUpdate>(PreFrameUpdate, typeof(PreUpdate));
        InsertSystem<IFrameUpdate>(0,FrameUpdate, typeof(Update));
        InsertSystem<IPreLateUpdate>(10, PreLateUpdate, typeof(PreLateUpdate));
        InsertSystem<IPostLateUpdate>(0, PostLateUpdate, typeof(PostLateUpdate));
    }

    /* EARLY */
    public static void Register(IEarlyUpdate _interface) 
    { 
        if (!_early.Contains(_interface))
        { 
            _early.Add(_interface); 
        }
    }
    public static void Unregister(IEarlyUpdate _interface) 
    {
        if (_early.Contains(_interface))
        { 
            _early.Remove(_interface);
        }
    }

    /* FIXED */
    public static void Register(IFixedUpdate _interface)
    {
        if (!_fixed.Contains(_interface))
        {
            _fixed.Add(_interface);
        }
    }
    public static void Unregister(IFixedUpdate _interface)
    {
        if (_fixed.Contains(_interface))
        {
            _fixed.Remove(_interface);
        }
    }

    /* PRE-FRAMEUPDATE */
    public static void Register(IPreFrameUpdate _interface)
    {
        if (!_preframe.Contains(_interface))
        {
            _preframe.Add(_interface);
        }
    }
    public static void Unregister(IPreFrameUpdate _interface)
    {
        if (_preframe.Contains(_interface))
        {
            _preframe.Remove(_interface);
        }
    }

    /* FRAMEUPDATE */
    public static void Register(IFrameUpdate _interface)
    {
        if (!_frame.Contains(_interface))
        {
            _frame.Add(_interface);
        }
    }
    public static void Unregister(IFrameUpdate _interface)
    {
        if (_frame.Contains(_interface))
        {
            _frame.Remove(_interface);
        }
    }

    /* PRE-LATEUPDATE */
    public static void Register(IPreLateUpdate _interface)
    {
        if (!_prelate.Contains(_interface))
        {
            _prelate.Add(_interface);
        }
    }
    public static void Unregister(IPreLateUpdate _interface)
    {
        if (_prelate.Contains(_interface))
        {
            _prelate.Remove(_interface);
        }
    }

    /* POST-LATEUPDATE */
    public static void Register(IPostLateUpdate _interface)
    {
        if (!_postlate.Contains(_interface))
        {
            _postlate.Add(_interface);
        }
    }
    public static void Unregister(IPostLateUpdate _interface)
    {
        if (_postlate.Contains(_interface))
        {
            _postlate.Remove(_interface);
        }
    }

    static List<IEarlyUpdate> _early = new List<IEarlyUpdate>();
    static List<IFixedUpdate> _fixed = new List<IFixedUpdate>();
    static List<IPreFrameUpdate> _preframe = new List<IPreFrameUpdate>();
    static List<IFrameUpdate> _frame = new List<IFrameUpdate>();
    static List<IPreLateUpdate> _prelate = new List<IPreLateUpdate>();
    static List<IPostLateUpdate> _postlate = new List<IPostLateUpdate>();

    static double EarlyTime = 0;
    static void EarlyUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _early.Count; i++)
        {
            try
            {
                _early[i]?.EarlyUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        EarlyTime = ts.TotalMilliseconds;
    }

    static double FixedTime = 0;
    static void FixedUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _fixed.Count; i++)
        {
            try
            {
                _fixed[i]?.FixedUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        FixedTime = ts.TotalMilliseconds;
    }

    static double PreFrameTime = 0;

    static void PreFrameUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _preframe.Count; i++)
        {
            try
            {
                _preframe[i]?.PreFrameUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        PreFrameTime = ts.TotalMilliseconds;
    }

    static double FrameTime = 0;

    static void FrameUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _frame.Count; i++)
        {
            try
            {
                _frame[i]?.FrameUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        FrameTime = ts.TotalMilliseconds;
    }

    static double PreLateTime = 0;

    static void PreLateUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _prelate.Count; i++)
        {
            try
            {
                _prelate[i]?.PreLateUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        PreLateTime = ts.TotalMilliseconds;
    }

    static double PostLateTime = 0;
    static void PostLateUpdate()
    {
        DateTime start = DateTime.Now;
        for (int i = 0; i < _postlate.Count; i++)
        {
            try
            {
                _postlate[i]?.PostLateUpdate();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        DateTime end = DateTime.Now;
        TimeSpan ts = (end - start);
        PostLateTime = ts.TotalMilliseconds;
    }

#if UNITY_EDITOR
    [MenuItem("Game/Loop/Dump Updates (Default)")]
    static void DumpUpdates()
    {
        using (StringWriter sw = new StringWriter())
        {
            sw.WriteLine("EarlyUpdate ["+ _early.Count+"] [time: "+EarlyTime+" ms]");
            sw.WriteLine("FixedUpdate [" + _fixed.Count + "] [time: " + FixedTime + " ms]");
            sw.WriteLine("PreUpdate [" + _preframe.Count + "] [time: " + PreFrameTime + " ms]");
            sw.WriteLine("FrameUpdate [" + _frame.Count + "] [time: " + FrameTime + " ms]");
            sw.WriteLine("PreLateUpdate [" + _prelate.Count + "] [time: " + PreLateTime + " ms]"); ;
            sw.WriteLine("PostLateUpdate [" + _postlate.Count + "] [time: " + PostLateTime + " ms]");

            File.WriteAllText(Path.Combine(Application.dataPath, "updates_dump.txt"), sw.ToString());
        }
    }

    [MenuItem("Game/Loop/Dump (Default)")]
    static void DumpPlayerLoopDefault()
    {
        PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();

        using (StringWriter sw = new StringWriter())
        {
            sw.WriteLine("Systems " + playerLoop.subSystemList.Length);

            //deep 1
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                sw.WriteLine("[" + i + "]" + playerLoop.subSystemList[i].type.Name);

                //deep 2
                if (playerLoop.subSystemList[i].subSystemList.Length > 0)
                {
                    for (int j = 0; j < playerLoop.subSystemList[i].subSystemList.Length; j++)
                    {
                        sw.WriteLine("      [" + j + "]" + playerLoop.subSystemList[i].subSystemList[j].type.Name);
                    }
                }
            }

            File.WriteAllText(Path.Combine(Application.dataPath, "default_systems_dump.txt"), sw.ToString());
        }
    }

    [MenuItem("Game/Loop/Dump (Current)")]
    static void DumpPlayerLoopCurrent()
    {
        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();

        using (StringWriter sw = new StringWriter())
        {
            sw.WriteLine("Systems " + playerLoop.subSystemList.Length);

            //deep 1
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                sw.WriteLine("[" + i + "]" + playerLoop.subSystemList[i].type.Name);

                //deep 2
                if (playerLoop.subSystemList[i].subSystemList.Length > 0)
                {
                    for (int j = 0; j < playerLoop.subSystemList[i].subSystemList.Length; j++)
                    {
                        sw.WriteLine("      [" + j + "]" + playerLoop.subSystemList[i].subSystemList[j].type.Name);
                    }
                }
            }

            File.WriteAllText(Path.Combine(Application.dataPath, "current_systems_dump.txt"), sw.ToString());
        }
    }
#endif
}
