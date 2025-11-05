using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlCrawler
{
    public class HtmlParser
    {
        private readonly string input;
        private int pos = 0;

        public HtmlParser(string html) { input = html ?? ""; }

        public HtmlDocument Parse()
        {
            HtmlNode? root = null;
            var stack = new Stack<HtmlNode>();

            while (!End())
            {
                if (Peek() == '<')
                {
                    if (Peek(1) == '/') 
                    {
                        Consume(); Consume(); 
                        var name = ReadUntil('>');
                        Consume(); 
                        if (stack.Count > 0) stack.Pop();
                    }
                    else
                    {
                        Consume();
                        var tag = ReadName();
                        var node = new HtmlNode(NodeType.Element) { TagName = tag };

                        SkipWhitespace();
                        while (!End() && Peek() != '>' && Peek() != '/')
                        {
                            var attrName = ReadName();
                            string attrVal = "";
                            SkipWhitespace();
                            if (Peek() == '=')
                            {
                                Consume(); SkipWhitespace();
                                var quote = Peek();
                                if (quote == '"' || quote == '\'')
                                {
                                    Consume();
                                    attrVal = ReadUntil((char)quote);
                                    Consume();
                                }
                                else
                                {
                                    attrVal = ReadName();
                                }
                            }
                            if (!string.IsNullOrEmpty(attrName))
                                node.Attributes.Add(new HtmlAttribute(attrName, attrVal));
                            SkipWhitespace();
                        }

                        bool selfClosing = false;
                        if (Peek() == '/')
                        {
                            selfClosing = true;
                            Consume();
                        }
                        if (Peek() == '>') Consume();

                        if (stack.Count > 0)
                        {
                            stack.Peek().AddChild(node);
                        }
                        else
                        {
                            root = node;
                        }

                        if (!selfClosing)
                        {
                            stack.Push(node);
                        }
                    }
                }
                else
                {
                    var text = ReadUntil('<');
                    if (!string.IsNullOrWhiteSpace(text) && stack.Count > 0)
                    {
                        var tn = new HtmlNode(NodeType.Text) { Text = text.Trim() };
                        stack.Peek().AddChild(tn);
                    }
                }
            }

            if (root == null)
                throw new Exception("No root element found in HTML.");

            return new HtmlDocument(root);
        }

        private bool End() => pos >= input.Length;
        private char Peek(int ahead = 0) => pos + ahead < input.Length ? input[pos + ahead] : '\0';
        private void Consume() { if (pos < input.Length) pos++; }
        private void SkipWhitespace() { while (!End() && char.IsWhiteSpace(Peek())) Consume(); }
        private string ReadUntil(char stop)
        {
            var sb = new StringBuilder();
            while (!End() && Peek() != stop) { sb.Append(Peek()); Consume(); }
            return sb.ToString();
        }
        private string ReadName()
        {
            SkipWhitespace();
            var sb = new StringBuilder();
            while (!End())
            {
                var c = Peek();
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ':') { sb.Append(c); Consume(); }
                else break;
            }
            return sb.ToString();
        }
    }
}
