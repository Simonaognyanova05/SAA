using System;
using System.Threading;

namespace Crawler
{
    public class ThreadNode
    {
        public Thread Value;
        public ThreadNode Next;

        public ThreadNode(Thread t)
        {
            Value = t;
        }
    }

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

        public MyList<HtmlNode> Find(HtmlNode root, string path)
        {
            MyList<HtmlNode> result = new MyList<HtmlNode>();
            if (root == null || path == null)
                return result;

            int pos = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/')
                pos = 2;

            PathPart parts = ParsePath(path, pos);

            Search(root, parts, result);

            return result;
        }

        private PathPart ParsePath(string path, int start)
        {
            PathPart head = null;
            PathPart tail = null;

            string curr = "";

            for (int i = start; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    if (curr != "")
                    {
                        PathPart p = new PathPart(curr);
                        if (head == null) head = p;
                        else tail.Next = p;
                        tail = p;
                        curr = "";
                    }
                }
                else
                {
                    curr += path[i];
                }
            }

            if (curr != "")
            {
                PathPart p = new PathPart(curr);
                if (head == null) head = p;
                else tail.Next = p;
                tail = p;
            }

            return head;
        }

        private void Search(HtmlNode node, PathPart part, MyList<HtmlNode> result)
        {
            if (node == null || part == null)
                return;

            ThreadNode tHead = null;
            ThreadNode tTail = null;

            HtmlNode child = node.FirstChild;

            int counter = 0;

            while (child != null)
            {
                HtmlNode local = child;
                int localCount = counter + 1;

                Thread t = new Thread(() =>
                {
                    if (Match(local, part.Text, localCount))
                    {
                        if (part.Next == null)
                        {
                            lock (locker)
                            {
                                result.Add(local);
                            }
                        }
                        else
                        {
                            Search(local, part.Next, result);
                        }
                    }
                });

                ThreadNode tn = new ThreadNode(t);
                if (tHead == null) tHead = tn;
                else tTail.Next = tn;
                tTail = tn;

                t.Start();

                counter++;         
                child = child.NextSibling;
            }

            ThreadNode cur = tHead;
            while (cur != null)
            {
                cur.Value.Join();
                cur = cur.Next;
            }
        }

        private bool Match(HtmlNode node, string pattern, int indexPos)
        {
            if (pattern == "*")
                return true;

            string tag = "";
            string attrName = "";
            string attrValue = "";
            int requiredIndex = -1;

            ParseStep(pattern, out tag, out attrName, out attrValue, out requiredIndex);

            if (tag != "" && !EqualsIgnoreCase(node.TagName, tag))
                return false;

            if (attrName != "")
            {
                string v = node.Attributes.Get(attrName);
                if (v == null || v != attrValue)
                    return false;
            }

            if (requiredIndex != -1 && requiredIndex != indexPos)
                return false;

            return true;
        }

        private void ParseStep(
            string s,
            out string tag,
            out string attrName,
            out string attrValue,
            out int index)
        {
            tag = "";
            attrName = "";
            attrValue = "";
            index = -1;

            int i = 0;

            while (i < s.Length && s[i] != '[')
            {
                tag += s[i];
                i++;
            }

            while (i < s.Length)
            {
                if (s[i] == '[')
                {
                    i++;

                    if (i < s.Length && s[i] == '@')
                    {
                        i++; 

                        while (i < s.Length && s[i] != '=')
                            attrName += s[i++];

                        i += 2; 

                        while (i < s.Length && s[i] != '\'')
                            attrValue += s[i++];

                        i++; 
                    }
                    else
                    {
                        string num = "";
                        while (i < s.Length && s[i] != ']')
                            num += s[i++];

                        index = ManualParseInt(num);
                    }
                }
                i++;
            }
        }

        private int ManualParseInt(string s)
        {
            int v = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] < '0' || s[i] > '9')
                    return -1;
                v = v * 10 + (s[i] - '0');
            }
            return v;
        }

        private bool EqualsIgnoreCase(string a, string b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                char c1 = a[i];
                char c2 = b[i];

                if (c1 >= 'A' && c1 <= 'Z') c1 = (char)(c1 + 32);
                if (c2 >= 'A' && c2 <= 'Z') c2 = (char)(c2 + 32);

                if (c1 != c2) return false;
            }
            return true;
        }
    }
}
