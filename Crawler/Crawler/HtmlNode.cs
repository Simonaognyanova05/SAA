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

        public void AddChild(HtmlNode child)
        {
            if (FirstChild == null)
                FirstChild = child;
            else
            {
                HtmlNode current = FirstChild;
                while (current.NextSibling != null)
                    current = current.NextSibling;
                current.NextSibling = child;
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
            Console.Write("<" + TagName + ">");

            if (Attributes.Count > 0)
            {
                Console.Write(" [");
                foreach (var kv in Attributes)
                    Console.Write($"{kv.Key}='{kv.Value}' ");
                Console.Write("]");
            }

            if (IsSelfClosing)
                Console.Write(" (self-closing)");

            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(InnerText))
            {
                for (int i = 0; i < indent + 1; i++)
                    Console.Write("  ");
                Console.WriteLine("Text: " + InnerText.Trim());
            }

            HtmlNode child = FirstChild;
            while (child != null)
            {
                child.Print(indent + 1);
                child = child.NextSibling;
            }
        }

        public string ToHtmlString()
        {
            if (IsSelfClosing)
                return "<" + TagName + MakeAttrString() + " />";

            string result = "<" + TagName + MakeAttrString() + ">";

            if (!string.IsNullOrWhiteSpace(InnerText))
                result += InnerText.Trim();

            HtmlNode child = FirstChild;
            while (child != null)
            {
                result += child.ToHtmlString();
                child = child.NextSibling;
            }

            result += "</" + TagName + ">";
            return result;
        }

        private string MakeAttrString()
        {
            if (Attributes.Count == 0) return "";
            string attrs = "";
            foreach (var kv in Attributes)
                attrs += $" {kv.Key}='{kv.Value}'";
            return attrs;
        }

        public HtmlNode ShallowCopy()
        {
            HtmlNode copy = new HtmlNode(this.TagName, this.IsSelfClosing);
            copy.InnerText = this.InnerText;
            copy.Attributes = this.Attributes;  
            copy.FirstChild = this.FirstChild;  
            copy.NextSibling = null;            
            return copy;
        }

        public HtmlNode DeepCopy()
        {
            HtmlNode copy = new HtmlNode(this.TagName, this.IsSelfClosing);
            copy.InnerText = this.InnerText;

            foreach (var kv in this.Attributes)
            {
                copy.Attributes[kv.Key] = kv.Value;
            }

            HtmlNode child = this.FirstChild;
            HtmlNode prevCopy = null;

            while (child != null)
            {
                HtmlNode childCopy = child.DeepCopy();
                if (copy.FirstChild == null)
                    copy.FirstChild = childCopy;
                else
                    prevCopy.NextSibling = childCopy;

                childCopy.Parent = copy;
                prevCopy = childCopy;
                child = child.NextSibling;
            }

            return copy;
        }
    }
}
