using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public HtmlNode(string tag, bool selfClosing = false)
        {
            TagName = tag;
            IsSelfClosing = selfClosing;
            InnerText = "";
        }

        public void AddChild(HtmlNode child)
        {
            if (FirstChild == null)
            {
                FirstChild = child;
            }
            else
            {
                HtmlNode current = FirstChild;
                while (current.NextSibling != null)
                {
                    current = current.NextSibling;
                }
                current.NextSibling = child;
            }
            child.Parent = this;
        }


        public void Print(int indent = 0)
        {
            for (int i = 0; i < indent; i++) 
                Console.Write("  ");
            Console.WriteLine("<" + TagName + "> " + (IsSelfClosing ? "(self-closing)" : ""));

            if (InnerText != null && InnerText.Trim() != "")
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
    }
}
