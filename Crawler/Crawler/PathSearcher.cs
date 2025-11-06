using System;
using System.Collections.Generic;

namespace Crawler
{
    public class PathSearcher
    {
        public List<HtmlNode> Find(HtmlNode root, string path)
        {
            List<HtmlNode> result = new List<HtmlNode>();

            if (root == null || path == null || path.Length == 0)
                return result;

            int startIndex = 0;
            if (path.Length >= 2 && path[0] == '/' && path[1] == '/')
                startIndex = 2;

            List<string> parts = new List<string>();
            string currentPart = "";
            for (int i = startIndex; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                    }
                }
                else
                {
                    currentPart += path[i];
                }
            }
            if (currentPart.Length > 0)
                parts.Add(currentPart);

            List<HtmlNode> currentLevel = new List<HtmlNode> { root };

            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];
                List<HtmlNode> nextLevel = new List<HtmlNode>();

                string tagName = "";
                string attrName = "";
                string attrValue = "";
                int index = -1;
                bool anyTag = false;

                if (part == "*")
                {
                    anyTag = true;
                }
                else
                {
                    int j = 0;
                    while (j < part.Length && part[j] != '[')
                    {
                        tagName += part[j];
                        j++;
                    }

                    while (j < part.Length)
                    {
                        if (part[j] == '[')
                        {
                            j++;
                            if (j < part.Length && part[j] == '@')
                            {
                                j++;
                                while (j < part.Length && part[j] != '=')
                                {
                                    attrName += part[j];
                                    j++;
                                }
                                j += 2; 
                                while (j < part.Length && part[j] != '\'')
                                {
                                    attrValue += part[j];
                                    j++;
                                }
                                j++; 
                                while (j < part.Length && part[j] != ']') j++;
                            }
                            else
                            {
                                string num = "";
                                while (j < part.Length && part[j] != ']')
                                {
                                    num += part[j];
                                    j++;
                                }
                                int parsed = 0;
                                bool ok = true;
                                for (int k = 0; k < num.Length; k++)
                                {
                                    if (num[k] < '0' || num[k] > '9') { ok = false; break; }
                                    parsed = parsed * 10 + (num[k] - '0');
                                }
                                if (ok) index = parsed;
                            }
                        }
                        j++;
                    }
                }

                foreach (var node in currentLevel)
                {
                    HtmlNode child = node.FirstChild;
                    int counter = 0;

                    while (child != null)
                    {
                        bool match = false;

                        if (anyTag) match = true;
                        else if (tagName.Length > 0 && child.TagName == tagName)
                            match = true;
                        
                        if (match && attrName.Length > 0)
                        {
                            if (child.Attributes != null &&
                                child.Attributes.ContainsKey(attrName) &&
                                child.Attributes[attrName] == attrValue)
                            {
                            }
                            else
                                match = false;
                        }

                        if (match)
                        {
                            counter++;
                            if (index == -1 || counter == index)
                            {
                                nextLevel.Add(child);
                            }
                        }

                        child = child.NextSibling;
                    }
                }

                currentLevel = nextLevel;
            }

            return currentLevel;
        }
    }
}
