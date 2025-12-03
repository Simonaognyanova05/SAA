using System;
// НЕ СЕ ИЗПОЛЗВАТ: System.Text, System.IO, System.Linq

namespace Crawler
{
    public static class SimpleCompressor
    {
        // ====================================================================
        // I. Ръчни Помощни Функции
        // ====================================================================

        private static bool IsHtmlSymbol(char c)
        {
            return c == '<' || c == '>' || c == '&' || c == ';';
        }

        private static string IntToString(int value)
        {
            if (value == 0) return "0";

            string result = "";
            int temp = value;
            int digit;

            while (temp > 0)
            {
                digit = temp % 10;
                char c = (char)('0' + digit);

                result = c + result;

                temp /= 10;
            }

            return result;
        }


        // ====================================================================
        // II. Алгоритми за RLE (Total Count N)
        // ====================================================================

        /// <summary>
        /// Ръчна RLE компресия: Символ + Общ Брой Повторения (N). 
        /// </summary>
        private static string CompressText(string input)
        {
            if (input == null || input.Length == 0) return "";

            string result = "";
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                // HTML символи И ЦИФРИ се записват директно.
                if (IsHtmlSymbol(c) || (c >= '0' && c <= '9'))
                {
                    result += c;
                    i++;
                    continue;
                }

                // RLE логика:
                int count = 1;
                int j = i + 1;

                while (j < input.Length && input[j] == c)
                {
                    count++;
                    j++;
                }

                // 1. Добавяме символа
                result += c;

                if (count > 1)
                {
                    // 2. Добавяме ОБЩИЯ брой повторения (N)
                    result += IntToString(count);
                }

                i = j;
            }

            return result;
        }

        /// <summary>
        /// Ръчна RLE декомпресия (Total Count N).
        /// </summary>
        private static string DecompressText(string input)
        {
            if (input == null || input.Length == 0) return "";

            string result = "";

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // HTML символи И ЦИФРИ се добавят директно.
                if (IsHtmlSymbol(c) || (c >= '0' && c <= '9'))
                {
                    result += c;
                    continue;
                }

                // 1. Намиране на брояч на ОБЩИЯ брой повторения (N_total)
                int k = i + 1;
                string countStr = "";
                while (k < input.Length && input[k] >= '0' && input[k] <= '9')
                {
                    countStr += input[k];
                    k++;
                }

                if (countStr.Length > 0)
                {
                    // 2. Ръчно парсване на N_total
                    int N_total = 0;
                    int powerOf10 = 1;
                    for (int j = countStr.Length - 1; j >= 0; j--)
                    {
                        N_total += (countStr[j] - '0') * powerOf10;
                        powerOf10 *= 10;
                    }

                    // 3. Добавяне на символа N_total пъти. (Това е най-чистата логика!)
                    for (int j = 0; j < N_total; j++)
                        result += c;

                    // 4. Преместване на индекса i, за да прескочим брояча
                    i = k - 1;
                }
                else
                {
                    // N = 1 случай: Няма брояч, добавяме символа веднъж
                    result += c;
                }
            }

            return result;
        }

        // ====================================================================
        // III. ПУБЛИЧНИ МЕТОДИ
        // ====================================================================

        public static string Compress(string input)
        {
            return CompressText(input);
        }

        public static string Decompress(string input)
        {
            return DecompressText(input);
        }
    }
}