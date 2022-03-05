using System;
using UnityEngine;

[Serializable]
public class FixedList<T>
{
    public FixedList(int capacity)
    {
        m_Elements = new T[capacity];
    }

    ~FixedList()
    {
        Array.Clear(m_Elements, 0, m_Elements.Length);
    }

    public int Capacity
    {
        get { return m_Elements.Length; }
    }

    public int Count
    {
        get { return m_Count; }
    }

    public bool Contains(T item)
    {
        if(item == null)
        {
            return false;
        }
        for(int i = 0; i < Count; i++)
        {
            if(m_Elements[i] != null)
            {
                return m_Elements[i].Equals(item);
            }
        }

        return false;
    }

    public void Add(T item)
    {
        var index = (m_First + m_Count) % m_Elements.Length;
        m_Elements[index] = item;

        if (m_Count == m_Elements.Length)
            m_First = (m_First + 1) % m_Elements.Length;
        else
            ++m_Count;
    }

    public void AddRange(T[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Add(items[i]);
        }
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf<T>(m_Elements, item, 0, Capacity);
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if (index >= m_Count || m_Count <= 0)
        {
            return;
        }
        m_Count--;
        m_Elements[index] = default(T);
    }

    public T[] GetRange(int start, int end)
    {
        Debug.Assert(start > end);

        T[] array = new T[end - start];

        Array.Copy(m_Elements, start, array,0, end - start);

        return array;
    }

    public void RemoveRange(int index, int count)
    {
        int i = m_Count;
        m_Count -= count;
        if (index < m_Count)
        {
            Array.Copy(m_Elements, index + count, m_Elements, index, m_Count - index);
        }
        Array.Clear(m_Elements, m_Count, count);
    }

    public void Clear()
    {
        m_First = m_Count = 0;
        Array.Clear(m_Elements, 0, m_Elements.Length);
    }

    public void FastClear()
    {
        m_First = 0;
        m_Count = 0;
    }


    public T this[int i]
    {
        get
        {
            Debug.Assert(i < m_Count);
            return m_Elements[(m_First + i) % m_Elements.Length];
        }
        set
        {
            Debug.Assert(i < m_Count);
            m_Elements[(m_First + i) % m_Elements.Length] = value;
        }
    }

    public T[] GetArray()
    {
        return m_Elements;
    }

    public int HeadIndex
    {
        get { return m_First; }
    }

    public void Reset(int headIndex, int count)
    {
        m_First = headIndex;
        m_Count = count;
    }

    int m_First;
    int m_Count;
    T[] m_Elements;
}
