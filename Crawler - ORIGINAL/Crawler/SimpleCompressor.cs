using System;
using System.Text;

namespace Crawler
{
    public static class SimpleCompressor
    {
        private static bool IsHtmlChar(char c)
        {
            return c == '<' || c == '>' || c == '"' || c == '\'' || c == '=' || c == '/';
        }

        private const char Marker = '~'; 

        public static string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < input.Length)
            { 
                char c = input[i];
                
                if (c == Marker)
                {
                    sb.Append(Marker); 
                    sb.Append(Marker); 
                    i++;
                    continue;
                }

                if (IsHtmlChar(c))
                {
                    sb.Append(c);
                    i++; 
                    continue; 
                }
                
                int j = i + 1;
                while (j < input.Length && input[j] == c)
                {
                    if (input[j] == Marker || IsHtmlChar(input[j])) break;
                    j++;
                }
                int count = j - i;

                if (count >= 5) 
                {
                    sb.Append(Marker);          
                    sb.Append(c);               
                    sb.Append(ManualIntToString(count)); 
                }  
                else
                {
                    for (int k = 0; k < count; k++)
                        sb.Append(c);
                }

                i = j;
            }

            return sb.ToString();
        }

        public static string Decompress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i]; 

                if (c == Marker)
                {
                    if (i + 1 < input.Length && input[i + 1] == Marker)
                    {
                        sb.Append(Marker);
                        i += 2;
                        continue;
                    }
                   
                    if (i + 1 >= input.Length)
                    {
                        sb.Append(Marker);
                        i++;
                        continue;
                    }

                    char sym = input[i + 1];
                    int j = i + 2;
                    string num = "";
                    while (j < input.Length && IsDigit(input[j]))
                    {
                        num += input[j];
                        j++;
                    }

                    if (num.Length == 0)
                    {
                        sb.Append(Marker);
                        sb.Append(sym);
                        i += 2;
                        continue;
                    }

                    int cnt = ManualParseInt(num);
                    if (cnt < 1) cnt = 1;

                    for (int k = 0; k < cnt; k++)
                        sb.Append(sym);

                    i = j;
                    continue;
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }

            return sb.ToString();
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static int ManualParseInt(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int v = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return 0;
                v = v * 10 + (c - '0');
            }
            return v;
        }

        private static string ManualIntToString(int x)
        {
            if (x == 0) return "0";
            if (x < 0) x = -x;

            char[] buf = new char[20];
            int p = 0;
            while (x > 0)
            {
                int d = x % 10;
                buf[p++] = (char)('0' + d);
                x /= 10;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = p - 1; i >= 0; i--)
                sb.Append(buf[i]);
            return sb.ToString();
        }
    }
}
