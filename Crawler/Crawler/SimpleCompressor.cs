using System;
using System.IO;
using System.Text;

namespace Crawler
{
    public static class SimpleCompressor
    {
        public static string Compress(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            StringBuilder output = new StringBuilder();
            char prev = input[0];
            int count = 1;

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == prev)
                {
                    count++;
                }
                else
                {
                    output.Append(prev);
                    if (count > 1)
                        output.Append(count);
                    prev = input[i];
                    count = 1;
                }
            }

            output.Append(prev);
            if (count > 1)
                output.Append(count);

            return output.ToString();
        }

        public static string Decompress(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            StringBuilder output = new StringBuilder();
            char currentChar = '\0';
            string countStr = "";

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsLetterOrDigit(currentChar) && char.IsDigit(c))
                {
                    countStr += c;
                }
                else
                {
                    if (currentChar != '\0')
                    {
                        int count = 1;
                        if (countStr != "")
                        {
                            int.TryParse(countStr, out count);
                        }
                        for (int j = 0; j < count; j++)
                            output.Append(currentChar);
                    }
                    currentChar = c;
                    countStr = "";
                }
            }

            if (currentChar != '\0')
            {
                int count = 1;
                if (countStr != "")
                    int.TryParse(countStr, out count);
                for (int j = 0; j < count; j++)
                    output.Append(currentChar);
            }

            return output.ToString();
        }
    }
}
