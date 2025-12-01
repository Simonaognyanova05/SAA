using System;

namespace Crawler
{
    public class PathSearcher
    {
        public MyList<HtmlNode> Find(HtmlNode root, string path)
        {
            MyList<HtmlNode> result = new MyList<HtmlNode>();

            if (root == null || path == null)
                return result;

            // ignore leading //
            int start = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/')
                start = 2;

            // parse the path manually into linked list PathPart
            PathPart parts = ParsePath(path, start);

            // current list of nodes being processed
            MyList<HtmlNode> current = new MyList<HtmlNode>();
            current.Add(root);

            PathPart part = parts;

            while (part != null)
            {
                MyList<HtmlNode> next = new MyList<HtmlNode>();

                // iterate current nodes
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

        // ===============================================================
        // parse path into linked list PathPart (without Split)
        // ===============================================================
        private PathPart ParsePath(string path, int start)
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

        // ===============================================================
        // Matching logic
        // pattern may be:
        //  - "div"
        //  - "p[3]"
        //  - "table[@id='x']"
        //  - "td[@class='x'][2]"
        //  - "*"
        // ===============================================================
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

            // tag must match
            if (tag != "" && !EqualsIgnoreCase(node.TagName, tag))
                return false;

            // count same-tag siblings
            tagCounter++;

            // index must match
            if (index != -1 && tagCounter != index)
                return false;

            // attribute match
            if (attrName != "")
            {
                string v = node.Attributes.Get(attrName);
                if (v == null || v != attrValue)
                    return false;
            }

            return true;
        }

        // ===============================================================
        // parse something like:   td[@id='x'][3]
        // ===============================================================
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

            // read tag
            while (i < part.Length && part[i] != '[')
            {
                tag += part[i];
                i++;
            }

            // read filters
            while (i < part.Length)
            {
                if (part[i] == '[')
                {
                    i++;

                    if (i < part.Length && part[i] == '@')
                    {
                        // attribute
                        i++;
                        while (i < part.Length && part[i] != '=')
                        {
                            attrName += part[i];
                            i++;
                        }

                        i += 2; // skip ='
                        while (i < part.Length && part[i] != '\'')
                        {
                            attrValue += part[i];
                            i++;
                        }
                        i++; // skip '

                        while (i < part.Length && part[i] != ']') i++;
                    }
                    else
                    {
                        // index
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
