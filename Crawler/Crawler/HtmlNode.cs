using System;

namespace Crawler
{
    public class HtmlNode
    {
        public string TagName;
        public bool IsSelfClosing;
        public string InnerText;

        public HtmlNode FirstChild;
        public HtmlNode NextSibling;
        public HtmlNode Parent;

        // ✔ ЗАМЕНЕНО: вече използваме AttributeList
        public AttributeList Attributes;

        public HtmlNode(string tag, bool selfClosing = false)
        {
            TagName = tag;
            IsSelfClosing = selfClosing;
            InnerText = "";
            Attributes = new AttributeList();   // ✔ правилно
        }

        private string ManualTrim(string s)
        {
            if (s == null) return "";

            int start = 0;
            int end = s.Length - 1;

            while (start <= end &&
                  (s[start] == ' ' || s[start] == '\t' || s[start] == '\r' || s[start] == '\n'))
                start++;

            while (end >= start &&
                  (s[end] == ' ' || s[end] == '\t' || s[end] == '\r' || s[end] == '\n'))
                end--;

            if (end < start) return "";

            char[] arr = new char[end - start + 1];
            int p = 0;
            for (int i = start; i <= end; i++)
                arr[p++] = s[i];

            return new string(arr);
        }

        private bool IsWhitespace(string s)
        {
            if (s == null) return true;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
                    return false;
            }
            return true;
        }

        public void AddChild(HtmlNode child)
        {
            if (FirstChild == null)
                FirstChild = child;
            else
            {
                HtmlNode cur = FirstChild;
                while (cur.NextSibling != null)
                    cur = cur.NextSibling;

                cur.NextSibling = child;
            }

            child.Parent = this;
        }

        // ✔ вече работи с AttributeList
        public string GetAttribute(string name)
        {
            return Attributes.Get(name);
        }

        public void Print(int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                Console.Write("  ");

            Console.Write("<" + TagName);

            // ✔ печат на linked-list атрибути
            HtmlAttribute curAttr = Attributes.Head;
            if (curAttr != null)
            {
                Console.Write(" [");
                while (curAttr != null)
                {
                    Console.Write($"{curAttr.Name}='{curAttr.Value}' ");
                    curAttr = curAttr.Next;
                }
                Console.Write("]");
            }

            if (IsSelfClosing)
            {
                Console.WriteLine(" (self-closing)>");
                return;
            }

            Console.WriteLine(">");

            if (!IsWhitespace(InnerText))
            {
                for (int i = 0; i < indent + 1; i++)
                    Console.Write("  ");
                Console.WriteLine("Text: " + ManualTrim(InnerText));
            }

            HtmlNode child = FirstChild;
            while (child != null)
            {
                child.Print(indent + 1);
                child = child.NextSibling;
            }

            for (int i = 0; i < indent; i++)
                Console.Write("  ");
            Console.WriteLine("</" + TagName + ">");
        }

        public string ToHtmlString()
        {
            if (IsSelfClosing)
                return "<" + TagName + MakeAttrString() + " />";

            string html = "<" + TagName + MakeAttrString() + ">";

            if (!IsWhitespace(InnerText))
                html += ManualTrim(InnerText);

            HtmlNode c = FirstChild;
            while (c != null)
            {
                html += c.ToHtmlString();
                c = c.NextSibling;
            }

            html += "</" + TagName + ">";

            return html;
        }

        private string MakeAttrString()
        {
            HtmlAttribute a = Attributes.Head;
            if (a == null) return "";

            string s = "";
            while (a != null)
            {
                s += $" {a.Name}='{a.Value}'";
                a = a.Next;
            }
            return s;
        }

        public HtmlNode ShallowCopy()
        {
            HtmlNode c = new HtmlNode(this.TagName, this.IsSelfClosing);
            c.InnerText = this.InnerText;

            // ✔ shallow copy uses original AttributeList reference
            c.Attributes = this.Attributes;

            // ✔ children are NOT copied (shallow)
            c.FirstChild = this.FirstChild;

            return c;
        }

        public HtmlNode DeepCopy()
        {
            HtmlNode c = new HtmlNode(this.TagName, this.IsSelfClosing);
            c.InnerText = this.InnerText;

            // ✔ deep copy на AttributeList
            HtmlAttribute attr = this.Attributes.Head;
            while (attr != null)
            {
                c.Attributes.Add(attr.Name, attr.Value);
                attr = attr.Next;
            }

            // ✔ deep copy на децата
            HtmlNode child = FirstChild;
            HtmlNode prev = null;

            while (child != null)
            {
                HtmlNode childCopy = child.DeepCopy();

                if (c.FirstChild == null)
                    c.FirstChild = childCopy;
                else
                    prev.NextSibling = childCopy;

                childCopy.Parent = c;
                prev = childCopy;
                child = child.NextSibling;
            }

            return c;
        }
    }
}
