using System;
using System.Text;

namespace Crawler
{
    public static class SimpleCompressor
    {
        // Символите на които никога не прилагаме компресия (HTML-символи)
        private static bool IsHtmlChar(char c)
        {
            return c == '<' || c == '>' || c == '"' || c == '\'' || c == '=' || c == '/';
        }

        // Маркер за RLE запис (sentinel). Ако входът съдържа '~', ще бъде "escape-нат" като '~~'.
        private const char Marker = '~';

        // =====================================================================
        // COMPRESS — използва маркер за RLE: "~<символ><брой>"
        // =====================================================================
        public static string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];

                // маркерът в оригинала трябва да се екейпва веднага
                if (c == Marker)
                {
                    sb.Append(Marker); // първи '~'
                    sb.Append(Marker); // втори '~' -> означава литерален '~' при декомпресия
                    i++;
                    continue;
                }

                // Никога не компресираме HTML-символи (оставяме ги буквално)
                if (IsHtmlChar(c))
                {
                    sb.Append(c);
                    i++;
                    continue;
                }

                // намери дължината на серията от същия символ
                int j = i + 1;
                while (j < input.Length && input[j] == c)
                {
                    // ако срещнем маркер или HTML-символ в серията - прекъсваме серията тук
                    if (input[j] == Marker || IsHtmlChar(input[j])) break;
                    j++;
                }
                int count = j - i;

                // политика: encode само ако count >= 5 (можете да промените прага)
                if (count >= 5)
                {
                    sb.Append(Marker);           // маркер
                    sb.Append(c);                // символ
                    sb.Append(ManualIntToString(count)); // брой като десетично число
                }
                else
                {
                    // копираме серията буквално
                    for (int k = 0; k < count; k++)
                        sb.Append(c);
                }

                i = j;
            }

            return sb.ToString();
        }

        // =====================================================================
        // DECOMPRESS — разчита само на маркера "~" за RLE; "~~" -> '~'
        // =====================================================================
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
                    // ако е двойно '~~' -> литерален '~'
                    if (i + 1 < input.Length && input[i + 1] == Marker)
                    {
                        sb.Append(Marker);
                        i += 2;
                        continue;
                    }

                    // иначе трябва да имаме RLE: ~ <symbol> <digits>
                    if (i + 1 >= input.Length)
                    {
                        // повреден запис — третиране като литерален '~'
                        sb.Append(Marker);
                        i++;
                        continue;
                    }

                    char sym = input[i + 1];
                    int j = i + 2;
                    // прочитаме число (може да има няколко цифри)
                    string num = "";
                    while (j < input.Length && IsDigit(input[j]))
                    {
                        num += input[j];
                        j++;
                    }

                    if (num.Length == 0)
                    {
                        // няма число след маркера — третиране като литерални символи "~X"
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
                    // нормален символ — добавяме буквално
                    sb.Append(c);
                    i++;
                }
            }

            return sb.ToString();
        }

        // =====================================================================
        // Малки помощни (без използване на библиотеки за парс/формат)
        // =====================================================================
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

            // съберем цифрите в обратен ред
            char[] buf = new char[20];
            int p = 0;
            while (x > 0)
            {
                int d = x % 10;
                buf[p++] = (char)('0' + d);
                x /= 10;
            }
            // обръщаме
            StringBuilder sb = new StringBuilder();
            for (int i = p - 1; i >= 0; i--)
                sb.Append(buf[i]);
            return sb.ToString();
        }
    }
}
