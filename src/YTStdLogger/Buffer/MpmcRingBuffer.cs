using System;
using System.Threading;

namespace YTStdLogger.Buffer;

/// <summary>
/// 有界 MPMC 环形队列。
/// 使用序号槽位算法，生产/消费路径仅依赖 Interlocked 与 Volatile。
/// </summary>
public sealed class MpmcRingBuffer<T> where T : class
{
    private struct Slot
    {
        public long Sequence;
        public T? Item;
    }

    private readonly Slot[] _slots;
    private readonly int _mask;
    private long _enqueuePos;
    private long _dequeuePos;

    /// <summary>
    /// 初始化队列。
    /// </summary>
    public MpmcRingBuffer(int capacity)
    {
        if (capacity < 2 || (capacity & (capacity - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _slots = new Slot[capacity];
        _mask = capacity - 1;
        for (int i = 0; i < capacity; i++)
        {
            _slots[i].Sequence = i;
        }
    }

    /// <summary>
    /// 尝试入队。
    /// </summary>
    public bool TryEnqueue(T item)
    {
        while (true)
        {
            long pos = Volatile.Read(ref _enqueuePos);
            ref Slot slot = ref _slots[pos & _mask];
            long seq = Volatile.Read(ref slot.Sequence);
            long diff = seq - pos;
            if (diff == 0)
            {
                if (Interlocked.CompareExchange(ref _enqueuePos, pos + 1, pos) == pos)
                {
                    slot.Item = item;
                    Volatile.Write(ref slot.Sequence, pos + 1);
                    return true;
                }

                continue;
            }

            if (diff < 0)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 批量出队。
    /// </summary>
    public int TryDequeueBatch(Span<T> destination)
    {
        int count = 0;
        for (int i = 0; i < destination.Length; i++)
        {
            if (!TryDequeue(out T? item))
            {
                break;
            }

            if (item is null)
            {
                break;
            }

            destination[count++] = item;
        }

        return count;
    }

    /// <summary>
    /// 尝试出队。
    /// </summary>
    public bool TryDequeue(out T? item)
    {
        while (true)
        {
            long pos = Volatile.Read(ref _dequeuePos);
            ref Slot slot = ref _slots[pos & _mask];
            long seq = Volatile.Read(ref slot.Sequence);
            long diff = seq - (pos + 1);
            if (diff == 0)
            {
                if (Interlocked.CompareExchange(ref _dequeuePos, pos + 1, pos) == pos)
                {
                    item = slot.Item;
                    slot.Item = null;
                    Volatile.Write(ref slot.Sequence, pos + _mask + 1);
                    return true;
                }

                continue;
            }

            if (diff < 0)
            {
                item = null;
                return false;
            }
        }
    }
}
