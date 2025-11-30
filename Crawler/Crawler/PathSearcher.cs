using System;
using System.Collections.Generic;

namespace Crawler
{
    public class PathSearcher
    {
        public List<HtmlNode> Find(HtmlNode root, string path)
        {
            List<HtmlNode> result = new List<HtmlNode>();

            if (root == null || string.IsNullOrEmpty(path))
                return result;

            // ---------------------------------------------------------
            // Премахваме начално "//"
            // ---------------------------------------------------------
            int start = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/')
                start = 2;

            // ---------------------------------------------------------
            // Разделяме частите по "/"
            // ---------------------------------------------------------
            List<string> parts = new List<string>();
            string cur = "";

            for (int i = start; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    if (cur.Length > 0)
                    {
                        parts.Add(cur);
                        cur = "";
                    }
                }
                else
                {
                    cur += path[i];
                }
            }
            if (cur.Length > 0) parts.Add(cur);

            // ---------------------------------------------------------
            // Започваме търсене от root
            // ---------------------------------------------------------
            List<HtmlNode> current = new List<HtmlNode>();
            current.Add(root);

            // ---------------------------------------------------------
            // Обработваме всяка част от пътя
            // ---------------------------------------------------------
            foreach (string part in parts)
            {
                // parse of part
                string tag = "";
                string attrName = "";
                string attrValue = "";
                int index = -1;
                bool wildcard = false;

                ParseStep(part, out tag, out attrName, out attrValue, out index, out wildcard);

                // преминаване към следващия слой
                List<HtmlNode> next = new List<HtmlNode>();

                foreach (HtmlNode n in current)
                {
                    HtmlNode child = n.FirstChild;
                    int counter = 0;

                    while (child != null)
                    {
                        bool match = false;

                        // match tag
                        if (wildcard)
                            match = true;
                        else if (tag.Length > 0 && child.TagName == tag)
                            match = true;

                        // match attribute
                        if (match && attrName.Length > 0)
                        {
                            string val = child.Attributes.Get(attrName);
                            if (val == null || val != attrValue)
                                match = false;
                        }

                        // index match
                        if (match)
                        {
                            counter++;
                            if (index == -1 || counter == index)
                                next.Add(child);
                        }

                        child = child.NextSibling;
                    }
                }

                current = next;
            }

            return current;
        }

        // ====================================================================
        // Разбива една стъпка от пътя: напр. div[@id='x'][3]
        // ====================================================================
        private void ParseStep(string part,
                              out string tag,
                              out string attrName,
                              out string attrValue,
                              out int index,
                              out bool wildcard)
        {
            tag = "";
            attrName = "";
            attrValue = "";
            index = -1;
            wildcard = false;

            if (part == "*")
            {
                wildcard = true;
                return;
            }

            int i = 0;

            // ---------------------- TAG ----------------------
            while (i < part.Length && part[i] != '[')
            {
                tag += part[i];
                i++;
            }

            // ---------------------- FILTERS ----------------------
            while (i < part.Length)
            {
                if (part[i] == '[')
                {
                    i++;

                    // атрибут
                    if (i < part.Length && part[i] == '@')
                    {
                        i++; // skip @
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

                        i++; // skip quote

                        while (i < part.Length && part[i] != ']') i++;
                    }
                    else
                    {
                        // индекс
                        string num = "";
                        while (i < part.Length && part[i] != ']')
                        {
                            num += part[i];
                            i++;
                        }

                        int parsed = TryParseIndex(num);
                        if (parsed > 0) index = parsed;
                    }
                }
                i++;
            }
        }

        private int TryParseIndex(string s)
        {
            int x = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] < '0' || s[i] > '9') return -1;
                x = x * 10 + (s[i] - '0');
            }
            return x;
        }
    }
}
