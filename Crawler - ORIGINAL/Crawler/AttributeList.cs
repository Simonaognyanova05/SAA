using System;

namespace Crawler
{
    public class HtmlAttribute
    {
        public string Name;
        public string Value;
        public HtmlAttribute Next;
    }

    public class AttributeList
    {
        public HtmlAttribute Head;

        public void Add(string name, string value)
        {
            HtmlAttribute a = new HtmlAttribute();
            a.Name = name;
            a.Value = value;

            if (Head == null)
            {
                Head = a;
                return;
            }

            HtmlAttribute cur = Head;
            while (cur.Next != null)
                cur = cur.Next;

            cur.Next = a;
        }

        public string Get(string name)
        {
            HtmlAttribute cur = Head;
            while (cur != null)
            {
                if (EqualsIgnoreCase(cur.Name, name))
                    return cur.Value;

                cur = cur.Next;
            }
            return null;
        }

        private bool EqualsIgnoreCase(string a, string b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                char c1 = a[i];
                char c2 = b[i];

                if (c1 >= 'A' && c1 <= 'Z') c1 = (char)(c1 + 32);
                if (c2 >= 'A' && c2 <= 'Z') c2 = (char)(c2 + 32);

                if (c1 != c2) return false;
            }

            return true;
        }
    }
}
