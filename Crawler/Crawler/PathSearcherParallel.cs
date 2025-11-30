using System;
using System.Threading;

namespace Crawler
{
    // Свързан списък за нишки (забранени са List<T>)
    public class ThreadNode
    {
        public Thread Value;
        public ThreadNode Next;

        public ThreadNode(Thread t)
        {
            Value = t;
        }
    }

    // Свързан списък за части на пътя
    public class PathPart
    {
        public string Text;
        public PathPart Next;

        public PathPart(string text)
        {
            Text = text;
        }
    }

    public class PathSearcherParallel
    {
        private object locker = new object();

        // =====================================================================
        // Главна функция
        // =====================================================================
        public MyList<HtmlNode> Find(HtmlNode root, string path)
        {
            MyList<HtmlNode> result = new MyList<HtmlNode>();

            if (root == null || path == null)
                return result;

            if (path == "//")
            {
                result.Add(root);
                return result;
            }

            // Премахваме начално "//"
            int start = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/')
                start = 2;

            PathPart parts = ParsePath(path, start);

            SearchLevelParallel(root, parts, result);

            return result;
        }

        // =====================================================================
        // Ръчно разбиване на пътя без List, Split или IndexOf
        // =====================================================================
        private PathPart ParsePath(string path, int startIndex)
        {
            PathPart head = null;
            PathPart tail = null;

            string current = "";

            for (int i = startIndex; i < path.Length; i++)
            {
                char c = path[i];

                if (c == '/')
                {
                    if (current != "")
                    {
                        PathPart p = new PathPart(current);
                        if (head == null) head = p;
                        else tail.Next = p;
                        tail = p;

                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (current != "")
            {
                PathPart p = new PathPart(current);
                if (head == null) head = p;
                else tail.Next = p;
                tail = p;
            }

            return head;
        }

        // =====================================================================
        // Паралелно търсене на текущо ниво
        // =====================================================================
        private void SearchLevelParallel(HtmlNode node, PathPart part, MyList<HtmlNode> result)
        {
            if (node == null || part == null)
                return;

            ThreadNode threadHead = null;
            ThreadNode threadTail = null;

            HtmlNode child = node.FirstChild;

            while (child != null)
            {
                HtmlNode local = child;

                Thread t = new Thread(() =>
                {
                    if (Match(local, part.Text))
                    {
                        if (part.Next == null)
                        {
                            lock (locker)
                                result.Add(local);
                        }
                        else
                        {
                            SearchLevelParallel(local, part.Next, result);
                        }
                    }
                });

                ThreadNode tn = new ThreadNode(t);
                if (threadHead == null) threadHead = tn;
                else threadTail.Next = tn;
                threadTail = tn;

                t.Start();

                child = child.NextSibling;
            }

            // Join всички нишки чрез собствен списък
            ThreadNode cur = threadHead;
            while (cur != null)
            {
                cur.Value.Join();
                cur = cur.Next;
            }
        }

        // =====================================================================
        // Сравняване на възел с шаблон: div[@id='x'][3]
        // =====================================================================
        private bool Match(HtmlNode node, string pattern)
        {
            if (pattern == "*") return true;

            string tag = "";
            string attrName = "";
            string attrValue = "";
            int index = -1;

            int i = 0;

            // прочитаме таг
            while (i < pattern.Length && pattern[i] != '[')
            {
                tag += pattern[i];
                i++;
            }

            // четем филтри
            while (i < pattern.Length)
            {
                if (pattern[i] == '[')
                {
                    i++;

                    // атрибут
                    if (i < pattern.Length && pattern[i] == '@')
                    {
                        i++;
                        while (i < pattern.Length && pattern[i] != '=')
                            attrName += pattern[i++];

                        i += 2; // ='

                        while (i < pattern.Length && pattern[i] != '\'')
                            attrValue += pattern[i++];

                        i++; // '
                    }
                    else
                    {
                        // индекс
                        string num = "";
                        while (i < pattern.Length && pattern[i] != ']')
                            num += pattern[i++];

                        index = ManualParseInt(num);
                    }
                }

                i++;
            }

            // проверка таг
            if (tag != "" && node.TagName != tag)
                return false;

            // проверка атрибут
            if (attrName != "")
            {
                string v = node.Attributes.Get(attrName);
                if (v == null || v != attrValue)
                    return false;
            }

            // проверка индекс
            if (index > 0)
            {
                HtmlNode p = node.Parent?.FirstChild;
                int count = 0;

                while (p != null)
                {
                    if (p.TagName == node.TagName)
                    {
                        count++;
                        if (p == node)
                            break;
                    }
                    p = p.NextSibling;
                }

                if (count != index)
                    return false;
            }

            return true;
        }

        private int ManualParseInt(string s)
        {
            int x = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return -1;
                x = x * 10 + (c - '0');
            }
            return x;
        }
    }
}
