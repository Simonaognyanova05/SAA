using System;

namespace Crawler
{
    public class MyList<T>
    {
        private class Node
        {
            public T Value;
            public Node Next;

            public Node(T v)
            {
                Value = v;
            }
        }

        private Node head;
        private Node tail;
        private int count;

        public int Count
        {
            get { return count; }
        }

        public void Add(T value)
        {
            Node n = new Node(value);

            if (head == null)
            {
                head = n;
                tail = n;
            }
            else
            {
                tail.Next = n;
                tail = n;
            }

            count++;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new Exception("Index out of range");

                Node cur = head;
                int i = 0;

                while (cur != null)
                {
                    if (i == index)
                        return cur.Value;

                    cur = cur.Next;
                    i++;
                }

                throw new Exception("Index not found");
            }
        }

        public System.Collections.Generic.IEnumerable<T> ToEnumerable()
        {
            Node cur = head;
            while (cur != null)
            {
                yield return cur.Value;
                cur = cur.Next;
            }
        }
    }
}
