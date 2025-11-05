using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlCrawler
{
    public static class CommandLineSplitter
    {
        public static List<string> Split(string input)
        {
            var res = new List<string>();
            if (string.IsNullOrEmpty(input)) return res;
            var sb = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';
            for (int i=0;i<input.Length;i++)
            {
                var c = input[i];
                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true; quoteChar = c; continue;
                }
                if (inQuotes && c == quoteChar)
                {
                    inQuotes = false; continue;
                }
                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (sb.Length>0) { res.Add(sb.ToString()); sb.Clear(); }
                }
                else sb.Append(c);
            }
            if (sb.Length>0) res.Add(sb.ToString());
            return res;
        }
    }
}
