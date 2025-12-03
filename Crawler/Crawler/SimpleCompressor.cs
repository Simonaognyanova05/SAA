using System.Text;

namespace Crawler
{
    public static class SimpleCompressor
    {
        // ============================================================
        // PUBLIC API USED BY ARCHIVER
        // ============================================================

        // Компресира цял HTML документ като текст (RLE)
        public static string Compress(string html)
        {
            return CompressText(html);
        }

        // Декомпресира цял HTML документ като текст
        public static string Decompress(string compressed)
        {
            return DecompressText(compressed);
        }


        // ============================================================
        // FOR TREE COMPRESSION (optional)
        // ============================================================

        public static void CompressHtml(HtmlNode root)
        {
            if (root == null) return;

            if (!string.IsNullOrEmpty(root.InnerText))
                root.InnerText = CompressText(root.InnerText);

            HtmlNode ch = root.FirstChild;
            while (ch != null)
            {
                CompressHtml(ch);
                ch = ch.NextSibling;
            }
        }

        public static void DecompressHtml(HtmlNode root)
        {
            if (root == null) return;

            if (!string.IsNullOrEmpty(root.InnerText))
                root.InnerText = DecompressText(root.InnerText);

            HtmlNode ch = root.FirstChild;
            while (ch != null)
            {
                DecompressHtml(ch);
                ch = ch.NextSibling;
            }
        }


        // ============================================================
        // RLE TEXT COMPRESSION
        // ============================================================

        private static string CompressText(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            StringBuilder sb = new StringBuilder();
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
                    sb.Append(prev);
                    if (count > 1) sb.Append(count);

                    prev = input[i];
                    count = 1;
                }
            }

            sb.Append(prev);
            if (count > 1) sb.Append(count);

            return sb.ToString();
        }


        private static string DecompressText(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            StringBuilder sb = new StringBuilder();
            char chr = '\0';
            string digits = "";

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c >= '0' && c <= '9')
                {
                    digits += c;
                }
                else
                {
                    if (chr != '\0')
                    {
                        int count = digits == "" ? 1 : ManualParseInt(digits);
                        for (int j = 0; j < count; j++)
                            sb.Append(chr);
                    }

                    chr = c;
                    digits = "";
                }
            }

            // финален блок
            if (chr != '\0')
            {
                int count = digits == "" ? 1 : ManualParseInt(digits);

                for (int j = 0; j < count; j++)
                    sb.Append(chr);
            }

            return sb.ToString();
        }


        // ============================================================
        // Manual Parser (NO int.Parse)
        // ============================================================

        private static int ManualParseInt(string s)
        {
            int x = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return 1;
                x = x * 10 + (c - '0');
            }
            return x;
        }
    }
}
