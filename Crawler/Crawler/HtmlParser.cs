using System;
using System.Collections.Generic;
using System.Text;

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
            StringBuilder buffer = new StringBuilder();

            while (i < html.Length)
            {
                char c = html[i];

                if (c == '<')
                {
                    if (buffer.Length > 0 && buffer.ToString().Trim() != "")
                    {
                        stack.Peek().InnerText += buffer.ToString();
                        buffer.Clear();
                    }

                    bool closing = false;
                    i++;

                    if (i < html.Length && html[i] == '/')
                    {
                        closing = true;
                        i++;
                    }

                    StringBuilder tagBuilder = new StringBuilder();
                    while (i < html.Length && html[i] != '>' && html[i] != ' ' && html[i] != '/')
                    {
                        tagBuilder.Append(html[i]);
                        i++;
                    }

                    string tag = tagBuilder.ToString().Trim();

                    if (closing)
                    {
                        HtmlNode closed = stack.Pop();
                        if (closed.TagName != tag)
                            throw new Exception("HTML грешка: несъответстващ таг </" + tag + ">");
                        while (i < html.Length && html[i] != '>') i++;
                        i++;
                        continue;
                    }

                    HtmlNode node = new HtmlNode(tag);
                    Dictionary<string, string> attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    while (i < html.Length && html[i] != '>')
                    {
                        while (i < html.Length && html[i] == ' ') i++;
                        if (i >= html.Length || html[i] == '>' || html[i] == '/') break;

                        StringBuilder name = new StringBuilder();
                        while (i < html.Length && html[i] != '=' && html[i] != '>' && html[i] != ' ')
                        {
                            name.Append(html[i]);
                            i++;
                        }

                        string attrName = name.ToString().Trim();

                        while (i < html.Length && (html[i] == ' ' || html[i] == '=')) i++;

                        if (i < html.Length && (html[i] == '"' || html[i] == '\''))
                        {
                            char quote = html[i];
                            i++;
                            StringBuilder value = new StringBuilder();
                            while (i < html.Length && html[i] != quote)
                            {
                                value.Append(html[i]);
                                i++;
                            }
                            i++;
                            attrs[attrName] = value.ToString();
                        }
                        else
                        {
                            StringBuilder value = new StringBuilder();
                            while (i < html.Length && html[i] != ' ' && html[i] != '>')
                            {
                                value.Append(html[i]);
                                i++;
                            }
                            attrs[attrName] = value.ToString();
                        }
                    }

                    bool selfClosing = false;
                    while (i < html.Length && html[i] != '>')
                    {
                        if (html[i] == '/')
                            selfClosing = true;
                        i++;
                    }
                    i++;

                    node.Attributes = attrs;
                    node.IsSelfClosing = selfClosing || Array.Exists(SelfClosingTags, t => t == tag);
                    stack.Peek().AddChild(node);

                    if (!node.IsSelfClosing)
                        stack.Push(node);
                }
                else
                {
                    buffer.Append(c);
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
