using System;

[Serializable]
public class RingBuffer<T>
{
    public RingBuffer(int capacity)
    {
        m_Elements = new T[capacity];
    }

    public RingBuffer(uint capacity)
    {
        m_Elements = new T[capacity];
    }

    public int Capacity
    {
        get { return m_Elements.Length; }
    }

    public int Count
    {
        get { return m_Count; }
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

    public void Clear()
    {
        m_First = 0;
        m_Count = 0;
    }

    public T this[int i]
    {
        get
        {
            //Debug.Assert(i < m_Count);
            return m_Elements[(m_First + i) % m_Elements.Length];
        }
        set
        {
            //Debug.Assert(i < m_Count);
            m_Elements[(m_First + i) % m_Elements.Length] = value;
        }
    }

    public T[] ToArray()
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
