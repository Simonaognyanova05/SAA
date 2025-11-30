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

        // =======================================================
        // PARSE HTML → HtmlNode (root)
        // =======================================================
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

                // ---------------------------------------------------
                // ТЕКСТОВ ВЪЗЕЛ
                // ---------------------------------------------------
                if (c != '<')
                {
                    buffer.Append(c);
                    i++;
                    continue;
                }

                // ако сме срещнали < → добавяме текста към текущия възел
                if (!IsWhitespace(buffer.ToString()))
                {
                    stack.Peek().InnerText += buffer.ToString();
                }
                buffer.Clear();

                // прескачаме '<'
                i++;

                // ---------------------------------------------------
                // КОМЕНТАР <!-- ... -->
                // ---------------------------------------------------
                if (i + 2 < html.Length && html[i] == '!' && html[i + 1] == '-' && html[i + 2] == '-')
                {
                    i += 3; // skip !--
                    while (i + 2 < html.Length &&
                           !(html[i] == '-' && html[i + 1] == '-' && html[i + 2] == '>'))
                    {
                        i++;
                    }
                    i += 3; // -- >
                    continue;
                }

                // ---------------------------------------------------
                // ЗАТВАРЯЩ ТАГ </...>
                // ---------------------------------------------------
                bool closing = false;
                if (i < html.Length && html[i] == '/')
                {
                    closing = true;
                    i++;
                }

                // четем името на тага
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

                // ---------------------------------------------------
                // Обработка на затварящ таг
                // ---------------------------------------------------
                if (closing)
                {
                    // прескачаме до >
                    while (i < html.Length && html[i] != '>') i++;
                    i++; // прескачаме '>'

                    HtmlNode closed = stack.Pop();

                    if (closed.TagName != tagName)
                        throw new Exception("HTML грешка: несъответстващ таг </" + tagName + ">");

                    continue;
                }

                // ---------------------------------------------------
                // АТРИБУТИ → AttributeList
                // ---------------------------------------------------
                AttributeList attributes = new AttributeList();

                while (i < html.Length && html[i] != '>' && html[i] != '/')
                {
                    // пропускане на спейсове
                    while (i < html.Length && html[i] == ' ') i++;
                    if (i >= html.Length || html[i] == '>' || html[i] == '/') break;

                    // име на атрибут
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

                    // пропускаме спейсове и '='
                    while (i < html.Length && (html[i] == ' ' || html[i] == '=')) i++;

                    // стойност на атрибута
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
                        i++; // пропускаме затварящата кавичка

                        attrVal = val.ToString();
                    }
                    else
                    {
                        // unquoted value
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

                    // добавяме в свързания списък
                    attributes.Add(attrName, attrVal);
                }

                // ---------------------------------------------------
                // проверка за "/>" self closing
                // ---------------------------------------------------
                bool selfClosing = false;

                while (i < html.Length && html[i] != '>')
                {
                    if (html[i] == '/')
                        selfClosing = true;
                    i++;
                }

                i++; // skip '>'

                // принудително такива тагове
                for (int s = 0; s < SelfClosingTags.Length; s++)
                {
                    if (SelfClosingTags[s] == tagName)
                    {
                        selfClosing = true;
                        break;
                    }
                }

                // ---------------------------------------------------
                // СЪЗДАВАНЕ НА НОВ ВЪЗЕЛ
                // ---------------------------------------------------
                HtmlNode node = new HtmlNode(tagName);
                node.Attributes = attributes;
                node.IsSelfClosing = selfClosing;

                // добавяне към текущия родител
                stack.Peek().AddChild(node);

                // само ако НЕ е self closing → натискаме в стека
                if (!selfClosing)
                {
                    stack.Push(node);
                }
            }

            // -------------------------------------------------------
            // КРАЙНА ПРОВЕРКА: незатворени тагове
            // -------------------------------------------------------
            HtmlNode lastNode = stack.Pop();
            if (!stack.IsEmpty())
                throw new Exception("HTML грешка: незатворен таг <" + lastNode.TagName + ">");

            return root;
        }
    }
}
