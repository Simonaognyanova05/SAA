using System;
using System.Text;

namespace Crawler
{
    public class HtmlParser
    {
        private static readonly string[] SelfClosingTags =
        {
            "img", "br", "hr", "input", "meta", "link"
        };

        private bool IsWhitespace(string s)
        {
            if (s == null) return true;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != ' ' && c != '\n' && c != '\r' && c != '\t')
                    return false;
            }
            return true;
        }

        public HtmlNode Parse(string html)
        {
            MyStack<HtmlNode> stack = new MyStack<HtmlNode>();

            HtmlNode root = new HtmlNode("root");
            stack.Push(root);

            int i = 0;
            StringBuilder buffer = new StringBuilder();

            while (i < html.Length)
            {
                char c = html[i];

                if (c != '<')
                {
                    buffer.Append(c);
                    i++;
                    continue;
                }

                if (!IsWhitespace(buffer.ToString()))
                {
                    stack.Peek().InnerText += buffer.ToString();
                }
                buffer.Clear();

                i++;

                if (i + 2 < html.Length && html[i] == '!' && html[i + 1] == '-' && html[i + 2] == '-')
                {
                    i += 3; 
                    while (i + 2 < html.Length &&
                           !(html[i] == '-' && html[i + 1] == '-' && html[i + 2] == '>'))
                    {
                        i++;
                    }
                    i += 3;
                    continue;
                }

                bool closing = false;
                if (i < html.Length && html[i] == '/')
                {
                    closing = true;
                    i++;
                }

                StringBuilder tagBuilder = new StringBuilder();
                while (i < html.Length)
                {
                    char t = html[i];
                    if (t == '>' || t == ' ' || t == '/')
                        break;

                    tagBuilder.Append(t);
                    i++;
                }

                string tagName = tagBuilder.ToString();
                if (tagName.Length == 0)
                    throw new Exception("Празно HTML име на таг.");

                if (closing)
                {
                    while (i < html.Length && html[i] != '>') i++;
                    i++;

                    HtmlNode closed = stack.Pop();

                    if (closed.TagName != tagName)
                        throw new Exception("HTML грешка: несъответстващ таг </" + tagName + ">");

                    continue;
                }

                AttributeList attributes = new AttributeList();

                while (i < html.Length && html[i] != '>' && html[i] != '/')
                {
                    while (i < html.Length && html[i] == ' ') i++;
                    if (i >= html.Length || html[i] == '>' || html[i] == '/') break;

                    StringBuilder nameB = new StringBuilder();
                    while (i < html.Length)
                    {
                        char ch = html[i];
                        if (ch == '=' || ch == ' ' || ch == '>' || ch == '/')
                            break;
                        nameB.Append(ch);
                        i++;
                    }

                    string attrName = nameB.ToString();

                    while (i < html.Length && (html[i] == ' ' || html[i] == '=')) i++;

                    string attrVal = "";

                    if (i < html.Length && (html[i] == '"' || html[i] == '\''))
                    {
                        char quote = html[i];
                        i++;

                        StringBuilder val = new StringBuilder();
                        while (i < html.Length && html[i] != quote)
                        {
                            val.Append(html[i]);
                            i++;
                        }
                        i++; 

                        attrVal = val.ToString();
                    }
                    else
                    {
                        StringBuilder val = new StringBuilder();
                        while (i < html.Length &&
                               html[i] != ' ' &&
                               html[i] != '>' &&
                               html[i] != '/')
                        {
                            val.Append(html[i]);
                            i++;
                        }
                        attrVal = val.ToString();
                    }

                    attributes.Add(attrName, attrVal);
                }

                bool selfClosing = false;

                while (i < html.Length && html[i] != '>')
                {
                    if (html[i] == '/')
                        selfClosing = true;
                    i++;
                }

                i++; 
                for (int s = 0; s < SelfClosingTags.Length; s++)
                {
                    if (SelfClosingTags[s] == tagName)
                    {
                        selfClosing = true;
                        break;
                    }
                }

                HtmlNode node = new HtmlNode(tagName);
                node.Attributes = attributes;
                node.IsSelfClosing = selfClosing;

                stack.Peek().AddChild(node);

                if (!selfClosing)
                {
                    stack.Push(node);
                }
            }

            HtmlNode lastNode = stack.Pop();
            if (!stack.IsEmpty())
                throw new Exception("HTML грешка: незатворен таг <" + lastNode.TagName + ">");

            return root;
        }
    }
}
