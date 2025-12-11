using System.Collections;
using System.Collections.Generic;

namespace Mechanics.PathFinding
{
    public class PriorityQueue<T> : IEnumerable<(long priority, T item)> //courtesy of gpt, Unity LTS doesn't have this mf
    {
        private readonly List<(long priority, T item)> heap = new();

        public int Count => heap.Count;

        public void Enqueue(T item, long priority)
        {
            heap.Add((priority, item));
            SiftUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            var item = heap[0].item;
            heap[0] = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            SiftDown(0);
            return item;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (heap[parent].priority <= heap[i].priority) break;

                (heap[i], heap[parent]) = (heap[parent], heap[i]);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int left = i * 2 + 1;
                int right = i * 2 + 2;
                int smallest = i;

                if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                    smallest = left;

                if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                    smallest = right;

                if (smallest == i) break;

                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                i = smallest;
            }
        }

        public IEnumerator<(long priority, T item)> GetEnumerator()
        {
            return heap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)heap).GetEnumerator();
        }
    }

}