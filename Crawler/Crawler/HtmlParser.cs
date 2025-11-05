using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class HtmlParser
    {
        private static string[] SelfClosingTags = { "img", "br", "hr", "input", "meta", "link" };

        public HtmlNode Parse(string html)
        {
            MyStack<HtmlNode> stack = new MyStack<HtmlNode>();
            HtmlNode root = new HtmlNode("root");
            stack.Push(root);

            int i = 0;
            string buffer = "";

            while (i < html.Length)
            {
                char c = html[i];

                if (c == '<')
                {
                    if (buffer.Trim() != "")
                    {
                        stack.Peek().InnerText += buffer;
                        buffer = "";
                    }

                    bool closing = false;
                    i++;

                    if (i < html.Length && html[i] == '/')
                    {
                        closing = true;
                        i++;
                    }

                    string tag = "";
                    while (i < html.Length && html[i] != '>' && html[i] != ' ' && html[i] != '/')
                    {
                        tag += html[i];
                        i++;
                    }

                    bool inQuotes = false;
                    bool selfClosing = false;

                    while (i < html.Length)
                    {
                        char ch = html[i];
                        if (ch == '"') inQuotes = !inQuotes;
                        if (!inQuotes)
                        {
                            if (ch == '/')
                                selfClosing = true;
                            if (ch == '>')
                            {
                                i++;
                                break;
                            }
                        }
                        i++;
                    }

                    if (closing)
                    {
                        HtmlNode current = stack.Pop();
                        if (current.TagName != tag)
                            throw new Exception("HTML грешка: несъответстващ таг </" + tag + ">");
                    }
                    else
                    {
                        HtmlNode node = new HtmlNode(tag, selfClosing);
                        stack.Peek().AddChild(node);
                        if (!selfClosing)
                            stack.Push(node);
                    }
                }
                else
                {
                    buffer += c;
                    i++;
                }
            }

            if (!stack.IsEmpty())
            {
                HtmlNode last = stack.Pop();
                if (!stack.IsEmpty())
                    throw new Exception("HTML грешка: незатворен таг <" + last.TagName + ">");
            }

            return root;
        }
    }
}
