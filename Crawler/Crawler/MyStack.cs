using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class MyStack<T>
    {
        private class StackNode
        {
            public T Value;
            public StackNode Next;
            public StackNode(T value, StackNode next)
            {
                Value = value;
                Next = next;
            }
        }

        private StackNode top;

        public void Push(T value)
        {
            top = new StackNode(value, top);
        }

        public T Pop()
        {
            if (top == null)
                throw new System.Exception("Стекът е празен");
            T value = top.Value;
            top = top.Next;
            return value;
        }

        public T Peek()
        {
            if (top == null)
                throw new System.Exception("Стекът е празен");
            return top.Value;
        }

        public bool IsEmpty()
        {
            return top == null;
        }
    }
}
