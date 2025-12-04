using System;

namespace Crawler
{
    public static class SimpleCompressor
    {
        private static bool IsHtmlChar(char c)
        {
            return c == '<' || c == '>' || c == '"' || c == '\'' || c == '=' || c == '/';
        }

        // =====================================================================
        //                    COMPRESS — серии ≥ 5
        // =====================================================================
        public static string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            string result = "";
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                // НИКГА не компресираме HTML-символи
                if (IsHtmlChar(c))
                {
                    result += c;
                    i++;
                    continue;
                }

                // Търсим поредица
                int count = 1;
                int j = i + 1;
                while (j < input.Length && input[j] == c)
                {
                    count++;
                    j++;
                }

                // Ако серията е >= 5 → RLE
                if (count >= 5)
                {
                    result += c + count.ToString();
                }
                else
                {
                    // Иначе — текстът се копира 1 към 1
                    for (int k = 0; k < count; k++)
                        result += c;
                }

                i = j;
            }

            return result;
        }

        // =====================================================================
        //                    DECOMPRESS — само ако има серия ≥ 5
        // =====================================================================
        public static string Decompress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            string result = "";
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                // HTML символи → директно
                if (IsHtmlChar(c))
                {
                    result += c;
                    i++;
                    continue;
                }

                // Ако следва цифра → може да е RLE, но трябва да проверим дали е серия >=5
                int j = i + 1;

                if (j < input.Length && char.IsDigit(input[j]))
                {
                    // прочитаме числото
                    string number = "";
                    while (j < input.Length && char.IsDigit(input[j]))
                    {
                        number += input[j];
                        j++;
                    }

                    int count = int.Parse(number);

                    if (count >= 5)
                    {
                        // истински RLE
                        for (int k = 0; k < count; k++)
                            result += c;

                        i = j;
                        continue;
                    }
                    else
                    {
                        // ❗ НЕ Е RLE → това е нормален текст като "Text2"
                        // връщаме оригиналния символ и числото
                        result += c;
                        result += number;
                        i = j;
                        continue;
                    }
                }

                // нормален символ
                result += c;
                i++;
            }

            return result;
        }
        private static string CleanText(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            string r = "";
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                // премахваме whitespace: space, tab, newline, CR
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    continue;

                r += c;
            }

            return r;
        }

    }

}
