using System;
using System.Collections.Generic;

namespace HtmlCrawler
{
    public static class QueryEngine
    {
        public static List<HtmlNode> Evaluate(HtmlNode root, string path)
        {
            if (path == null || path.Length == 0 || path == "//")
            {
                var only = new List<HtmlNode>(1);
                only.Add(root);
                return only;
            }

            var results = new List<HtmlNode>();
            if (path.Length > 2 && path[0] == '/' && path[1] == '/')
            {
                var seg = path.Substring(2);
                var parsed = ParseSegment(seg);
                CollectAnywhere(root, parsed.name, parsed.idx, parsed.attrName, parsed.attrVal, results);
                return results;
            }

            // Ръчно разделяне на пътя (без Split)
            var segments = new List<string>();
            int start = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    if (i > start)
                        segments.Add(path.Substring(start, i - start));
                    start = i + 1;
                }
            }
            if (start < path.Length)
                segments.Add(path.Substring(start));

            var current = new List<HtmlNode>(8);
            current.Add(root);

            foreach (var seg in segments)
            {
                var next = new List<HtmlNode>(8);
                var parsed = ParseSegment(seg);
                for (int i = 0; i < current.Count; i++)
                {
                    var node = current[i];
                    var children = node.Children;
                    for (int j = 0; j < children.Count; j++)
                    {
                        var child = children[j];
                        if (Matches(child, parsed.name, parsed.attrName, parsed.attrVal))
                        {
                            next.Add(child);
                        }
                    }
                }

                if (parsed.idx > 0)
                {
                    var selected = new List<HtmlNode>(1);
                    int target = parsed.idx - 1;
                    if (target < next.Count)
                        selected.Add(next[target]);
                    current = selected;
                }
                else
                {
                    current = next;
                }
            }
            return current;
        }

        private static void CollectAnywhere(HtmlNode node, string name, int idx, string attrName, string attrVal, List<HtmlNode> results)
        {
            var children = node.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (Matches(child, name, attrName, attrVal))
                    results.Add(child);
                CollectAnywhere(child, name, idx, attrName, attrVal, results);
            }

            // Ако имаме индекс и вече сме събрали достатъчно резултати
            if (idx > 0 && results.Count >= idx)
            {
                var only = results[idx - 1];
                results.Clear();
                results.Add(only);
            }
        }



        private static (string name, int idx, string attrName, string attrVal) ParseSegment(string s)
        {
            string name = s;
            int idx = 0;
            string attrName = null;
            string attrVal = null;

            int len = s.Length;
            int bracket = -1;
            for (int i = 0; i < len; i++)
            {
                if (s[i] == '[')
                {
                    bracket = i;
                    break;
                }
            }

            if (bracket == -1)
                return (s, 0, null, null);

            name = s.Substring(0, bracket);
            int close = s.IndexOf(']', bracket + 1);
            if (close == -1) close = len - 1;

            // [@attr='val']
            if (bracket + 1 < len && s[bracket + 1] == '@')
            {
                int eq = s.IndexOf('=', bracket + 2);
                if (eq != -1)
                {
                    attrName = s.Substring(bracket + 2, eq - (bracket + 2));
                    int quote1 = s.IndexOf('\'', eq + 1);
                    int quote2 = s.LastIndexOf('\'');
                    if (quote1 != -1 && quote2 > quote1)
                        attrVal = s.Substring(quote1 + 1, quote2 - quote1 - 1);
                }
            }
            // [number]
            else
            {
                int numStart = bracket + 1;
                int numEnd = close;
                int val = 0;
                for (int i = numStart; i < numEnd; i++)
                {
                    int c = s[i] - '0';
                    if (c >= 0 && c <= 9)
                        val = val * 10 + c;
                    else break;
                }
                idx = val;
            }

            return (name, idx, attrName, attrVal);
        }

        private static bool Matches(HtmlNode node, string name, string attrName, string attrVal)
        {
            if (node.Type != NodeType.Element)
                return false;

            if (name.Length > 0 && name != "*" && !EqualIgnoreCase(node.TagName, name))
                return false;

            if (attrName != null)
            {
                var attrs = node.Attributes;
                for (int i = 0; i < attrs.Count; i++)
                {
                    var a = attrs[i];
                    if (EqualIgnoreCase(a.Name, attrName))
                    {
                        if (a.Value == attrVal)
                            return true;
                        return false;
                    }
                }
                return false;
            }

            return true;
        }

        private static bool EqualIgnoreCase(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                int c1 = a[i];
                int c2 = b[i];
                if (c1 >= 'A' && c1 <= 'Z') c1 += 32;
                if (c2 >= 'A' && c2 <= 'Z') c2 += 32;
                if (c1 != c2) return false;
            }
            return true;
        }
    }
}
