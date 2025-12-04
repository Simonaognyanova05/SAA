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

        public AttributeList Attributes;

        public HtmlNode(string tag, bool selfClosing = false)
        {
            TagName = tag;
            IsSelfClosing = selfClosing;
            InnerText = "";
            Attributes = new AttributeList();
        }

        private string ManualTrim(string s)
        {
            if (s == null) return "";
            int start = 0;
            int end = s.Length - 1;

            while (start <= end && (s[start] <= ' ')) start++;
            while (end >= start && (s[end] <= ' ')) end--;

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
                if (s[i] > ' ') return false;

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

        public string GetAttribute(string name)
        {
            return Attributes.Get(name);
        }

        public void Print(int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                Console.Write("  ");

            Console.Write("<" + TagName);

            HtmlAttribute a = Attributes.Head;
            while (a != null)
            {
                Console.Write($" {a.Name}='{a.Value}'");
                a = a.Next;
            }

            if (IsSelfClosing)
            {
                Console.WriteLine("/>");
                return;
            }

            Console.WriteLine(">");

            if (!IsWhitespace(InnerText))
            {
                for (int i = 0; i < indent + 1; i++)
                    Console.Write("  ");
                Console.WriteLine(ManualTrim(InnerText));
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
            string html = "<" + TagName + MakeAttrString();

            if (IsSelfClosing)
                return html + " />";

            html += ">";

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
            
            HtmlAttribute a = this.Attributes.Head;
            while (a != null)
            {
                c.Attributes.Add(a.Name, a.Value);
                a = a.Next;
            }

            return c;
        }

        public HtmlNode DeepCopy()
        {
            HtmlNode c = new HtmlNode(this.TagName, this.IsSelfClosing);
            c.InnerText = this.InnerText;

            HtmlAttribute attr = this.Attributes.Head;
            while (attr != null)
            {
                c.Attributes.Add(attr.Name, attr.Value);
                attr = attr.Next;
            }

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
