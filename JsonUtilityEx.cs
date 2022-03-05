using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityEx
{
    public static T[] ArrayFromJson<T>(string json)
    {
        ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
        return wrapper.Array;
    }

    public static string ToJson<T>(T[] array)
    {
        ArrayWrapper<T> wrapper = new ArrayWrapper<T>();
        wrapper.Array = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        ArrayWrapper<T> wrapper = new ArrayWrapper<T>();
        wrapper.Array = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class ArrayWrapper<T>
    {
        public T[] Array;
    }


    public static List<T> ListFromJson<T>(string json)
    {
        ListWrapper<T> wrapper = JsonUtility.FromJson<ListWrapper<T>>(json);
        return wrapper.List;
    }

    public static string ToJson<T>(List<T> array)
    {
        ListWrapper<T> wrapper = new ListWrapper<T>();
        wrapper.List = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(List<T> array, bool prettyPrint)
    {
        ListWrapper<T> wrapper = new ListWrapper<T>();
        wrapper.List = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> List;
    }
}
