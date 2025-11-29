using System;
using System.Collections.Generic;

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
        public Dictionary<string, string> Attributes;

        public HtmlNode(string tag, bool selfClosing = false)
        {
            TagName = tag;
            IsSelfClosing = selfClosing;
            InnerText = "";
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private string ManualTrim(string s)
        {
            if (s == null) return "";

            int start = 0;
            int end = s.Length - 1;

            while (start <= end)
            {
                char c = s[start];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    start++;
                else break;
            }

            while (end >= start)
            {
                char c = s[end];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    end--;
                else break;
            }

            int len = end - start + 1;
            if (len <= 0) return "";

            char[] arr = new char[len];
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

        public string GetAttribute(string name)
        {
            return Attributes.ContainsKey(name) ? Attributes[name] : null;
        }

        public void Print(int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                Console.Write("  ");

            Console.Write("<" + TagName);

            if (Attributes.Count > 0)
            {
                Console.Write(" [");
                foreach (var kv in Attributes)
                    Console.Write($"{kv.Key}='{kv.Value}' ");
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
            if (Attributes.Count == 0) return "";
            string s = "";
            foreach (var kv in Attributes)
                s += $" {kv.Key}='{kv.Value}'";
            return s;
        }

        public HtmlNode ShallowCopy()
        {
            HtmlNode c = new HtmlNode(this.TagName, this.IsSelfClosing);
            c.InnerText = this.InnerText;
            c.Attributes = this.Attributes; 
            c.FirstChild = this.FirstChild; 
            return c;
        }

        public HtmlNode DeepCopy()
        {
            HtmlNode c = new HtmlNode(this.TagName, this.IsSelfClosing);
            c.InnerText = this.InnerText;

            foreach (var kv in Attributes)
                c.Attributes[kv.Key] = kv.Value;

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
