using System;
using System.Runtime.InteropServices;

namespace Crawler
{
    public class PathSearcher
    {
        public MyList<HtmlNode> Find(HtmlNode root, string path)
        {
            MyList<HtmlNode> result = new MyList<HtmlNode>();

            if (root == null || path == null)
                return result;
            
            int start = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/') 
                start = 2;

            PathPart parts = ParsePath(path, start);
            
            MyList<HtmlNode> current = new MyList<HtmlNode>();
            current.Add(root);

            PathPart part = parts;

            while (part != null)
            {
                MyList<HtmlNode> next = new MyList<HtmlNode>();

                foreach (HtmlNode node in current.ToEnumerable())
                {
                    HtmlNode child = node.FirstChild;
                    int sameTagCount = 0;

                    while (child != null)
                    {
                        if (Match(child, part.Text, ref sameTagCount))
                        {
                            next.Add(child);
                        }

                        child = child.NextSibling;
                    }
                }

                current = next;
                part = part.Next;
            }

            return current;
        }

        private PathPart ParsePath(string path, int start) // 
        {
            PathPart head = null;
            PathPart tail = null;

            string cur = "";

            for (int i = start; i < path.Length; i++)
            {
                char c = path[i];

                if (c == '/')
                {
                    if (cur != "")
                    {
                        PathPart p = new PathPart(cur);
                        if (head == null) head = p;
                        else tail.Next = p;
                        tail = p;

                        cur = "";
                    }
                }
                else
                {
                    cur += c;
                }
            }

            if (cur != "")
            {
                PathPart p = new PathPart(cur);
                if (head == null) head = p;
                else tail.Next = p;
                tail = p;
            }

            return head;
        }

        private bool Match(HtmlNode node, string pattern, ref int tagCounter)
        {
            if (pattern == "*")
            {
                tagCounter++;
                return true;
            }

            string tag = "";
            string attrName = "";
            string attrValue = "";
            int index = -1;

            ParseStep(pattern, out tag, out attrName, out attrValue, out index);

            if (tag != "" && !EqualsIgnoreCase(node.TagName, tag))
                return false;

            tagCounter++;

            if (index != -1 && tagCounter != index)
                return false;

            if (attrName != "")
            {
                string v = node.Attributes.Get(attrName);
                if (v == null || v != attrValue)
                    return false;
            }

            return true;
        }

        private void ParseStep(string part,
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

            while (i < part.Length && part[i] != '[')
            {
                tag += part[i];
                i++;
            }

            while (i < part.Length)
            {
                if (part[i] == '[')
                {
                    i++;

                    if (i < part.Length && part[i] == '@')
                    {
                        i++;
                        while (i < part.Length && part[i] != '=')
                        {
                            attrName += part[i];
                            i++;
                        }

                        i += 2; 
                        while (i < part.Length && part[i] != '\'')
                        {
                            attrValue += part[i];
                            i++;
                        }
                        i++;

                        while (i < part.Length && part[i] != ']') i++;
                    }
                    else
                    {
                        string num = "";
                        while (i < part.Length && part[i] != ']')
                        {
                            num += part[i];
                            i++;
                        }
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
                char c = s[i];
                if (c < '0' || c > '9') return -1;
                v = v * 10 + (c - '0');
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
