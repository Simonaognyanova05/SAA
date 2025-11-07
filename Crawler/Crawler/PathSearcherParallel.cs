using System;
using System.Collections.Generic;
using System.Threading;

namespace Crawler
{
    public class PathSearcherParallel
    {
        private object locker = new object();

        public List<HtmlNode> Find(HtmlNode root, string path)
        {
            List<HtmlNode> result = new List<HtmlNode>();

            if (path == "//")
            {
                result.Add(root);
                return result;
            }

            if (path.StartsWith("//"))
                path = path.Substring(2);

            string[] parts = SplitPath(path);
            SearchLevelParallel(root, parts, 0, result);
            return result;
        }

        private string[] SplitPath(string path)
        {
            List<string> parts = new List<string>();
            string current = "";

            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                if (c == '/')
                {
                    if (current != "")
                    {
                        parts.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (current != "")
                parts.Add(current);

            return parts.ToArray();
        }

        private void SearchLevelParallel(HtmlNode node, string[] parts, int level, List<HtmlNode> result)
        {
            if (node == null) return;
            if (level >= parts.Length) return;

            HtmlNode child = node.FirstChild;
            List<Thread> threads = new List<Thread>();

            while (child != null)
            {
                HtmlNode localChild = child;

                Thread t = new Thread(() =>
                {
                    if (Match(localChild, parts[level]))
                    {
                        if (level == parts.Length - 1)
                        {
                            lock (locker)
                                result.Add(localChild);
                        }
                        else
                        {
                            SearchLevelParallel(localChild, parts, level + 1, result);
                        }
                    }
                });

                threads.Add(t);
                t.Start();
                child = child.NextSibling;
            }

            foreach (var t in threads)
                t.Join();
        }

        private bool Match(HtmlNode node, string pattern)
        {
            if (pattern == "*") return true;

            string tag = "";
            string attrName = "";
            string attrValue = "";
            int index = -1;

            int i = 0;
            while (i < pattern.Length && pattern[i] != '[')
            {
                tag += pattern[i];
                i++;
            }

            while (i < pattern.Length)
            {
                if (pattern[i] == '[')
                {
                    i++;
                    if (i < pattern.Length && pattern[i] == '@')
                    {
                        i++;
                        while (i < pattern.Length && pattern[i] != '=')
                            attrName += pattern[i++];
                        i += 2; 
                        while (i < pattern.Length && pattern[i] != '\'')
                            attrValue += pattern[i++];
                        i++;
                    }
                    else
                    {
                        string idxStr = "";
                        while (i < pattern.Length && pattern[i] != ']')
                            idxStr += pattern[i++];
                        int.TryParse(idxStr, out index);
                    }
                }
                i++;
            }

            if (tag != "" && tag != node.TagName)
                return false;

            if (attrName != "")
            {
                string val = node.GetAttribute(attrName);
                if (val == null || val != attrValue)
                    return false;
            }

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
    }
}
