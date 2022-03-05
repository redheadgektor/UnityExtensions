[System.Serializable]
public class FixedQueue<T>
{
    T[] items;
    int head;
    int tail;
    public int Capacity { get; private set; } = 0;

    public int Count
    {
        get
        {
            return tail - head;
        }
    }

    public bool IsFull
    {
        get
        {
            return Count >= Capacity;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return head == tail;
        }
    }

    public FixedQueue(int maxItems)
    {
        items = new T[maxItems];
        Capacity = maxItems;
        Clear();
    }

    public T this[int i]
    {
        get
        {
            return items[i % Count];
        }
        set
        {
            items[i % Count] = value;
        }
    }

    public T[] ToArray()
    {
        return items;
    }

    public void Clear()
    {
        head = 0;
        tail = 0;
    }

    public void Enqueue(T item)
    {
        items[tail % items.Length] = item;
        tail++;
    }

    public void Enqueue(T[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Enqueue(items[i]);
        }
    }

    public T Dequeue()
    {
        if (Count <= 0)
            return default;

        T element = items[head % items.Length];
        head++;
        return element;
    }

    public void Dequeue_Fake()
    {
        if (Count <= 0)
            return;

        head++;
    }
}