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
        }

        // Позволява обхождане с foreach
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
