using System;
using System.IO;
using System.Text;

namespace Crawler
{
    // Simple single-file archive for HTML + resources (images).
    // Format (human-friendly, no external libs):
    //
    // [SAA]
    // [HTML_SIZE]
    // <decimal length of compressed HTML>\n
    // [HTML_COMPRESSED]
    // <compressed HTML bytes (UTF8)>   <- exactly HTML_SIZE characters
    //
    // Then for each file:
    // [FILE]
    // <file path or name>\n
    // [SIZE]
    // <decimal byte length>\n
    // [DATA]
    // <HEX bytes: 2 chars per byte, uppercase>\n
    //
    // [END]
    //
    // All parsing/writing done manually (no Split/IndexOf/Regex/LINQ).
    public static class SimpleArchive
    {
        // Save the HTML tree and all images referenced by <img src="...">
        public static void Save(string archivePath, HtmlNode root)
        {
            if (archivePath == null) throw new ArgumentNullException(nameof(archivePath));
            if (root == null) throw new ArgumentNullException(nameof(root));

            // 1) produce HTML and compress it (textual)
            string html = root.ToHtmlString();
            string compressedHtml = SimpleCompressor.Compress(html);

            // 2) gather image files referenced by img src attributes
            MyList<HtmlNode> images = CollectImageNodes(root);

            using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
            {
                // Header
                w.WriteLine("[SAA]");

                // HTML block
                w.WriteLine("[HTML_SIZE]");
                w.WriteLine(ManualIntToString(compressedHtml.Length));
                w.WriteLine("[HTML_COMPRESSED]");
                w.Write(compressedHtml);
                w.WriteLine(); // ensure newline after html block

                // For each image, write file metadata + hex data
                for (int i = 0; i < images.Count; i++)
                {
                    HtmlNode img = images[i];
                    string src = img.Attributes.Get("src");
                    if (src == null) continue;

                    // Only pack if file exists
                    if (!File.Exists(src)) continue;

                    byte[] data = File.ReadAllBytes(src);

                    w.WriteLine("[FILE]");
                    w.WriteLine(src);
                    w.WriteLine("[SIZE]");
                    w.WriteLine(ManualIntToString(data.Length));
                    w.WriteLine("[DATA]");
                    WriteHex(data, w);
                }

                w.WriteLine("[END]");
            }
        }

        // Load archive, returns HtmlNode root; files are restored to disk with original names.
        public static HtmlNode Load(string archivePath)
        {
            if (archivePath == null) throw new ArgumentNullException(nameof(archivePath));
            if (!File.Exists(archivePath)) throw new FileNotFoundException("Archive not found", archivePath);

            string content = File.ReadAllText(archivePath, Encoding.UTF8);
            int pos = 0;

            Expect(content, ref pos, "[SAA]");

            // Read HTML
            Expect(content, ref pos, "[HTML_SIZE]");
            int htmlSize = ReadIntLine(content, ref pos);

            Expect(content, ref pos, "[HTML_COMPRESSED]");
            string compressedHtml = ReadBlock(content, ref pos, htmlSize);

            // decompress and parse
            string html = SimpleCompressor.Decompress(compressedHtml);
            HtmlParser parser = new HtmlParser();
            HtmlNode root = parser.Parse(html);

            // Now read files until [END]
            while (true)
            {
                SkipNewLines(content, ref pos);
                if (LookAhead(content, pos, "[END]"))
                {
                    // consume [END]
                    Expect(content, ref pos, "[END]");
                    break;
                }

                // expect [FILE]
                Expect(content, ref pos, "[FILE]");
                string filename = ReadLine(content, ref pos);

                Expect(content, ref pos, "[SIZE]");
                int size = ReadIntLine(content, ref pos);

                Expect(content, ref pos, "[DATA]");
                byte[] data = ReadHexBytes(content, ref pos, size);

                // write file to disk (overwrite if exists)
                File.WriteAllBytes(filename, data);
            }

            return root;
        }

        // ---------------- helper: collect <img> nodes recursively ----------------
        private static MyList<HtmlNode> CollectImageNodes(HtmlNode root)
        {
            MyList<HtmlNode> list = new MyList<HtmlNode>();
            if (root == null) return list;
            CollectImagesRec(root, list);
            return list;
        }

        private static void CollectImagesRec(HtmlNode node, MyList<HtmlNode> list)
        {
            if (node == null) return;
            if (EqualsIgnoreCase(node.TagName, "img"))
                list.Add(node);

            HtmlNode c = node.FirstChild;
            while (c != null)
            {
                CollectImagesRec(c, list);
                c = c.NextSibling;
            }
        }

        // ---------------- write/read hex for binary data (2 chars per byte) ---------------
        private static void WriteHex(byte[] data, StreamWriter w)
        {
            // write all bytes as HEX uppercase, then newline
            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i];
                char h1 = NibbleToHex((b >> 4) & 0xF);
                char h2 = NibbleToHex(b & 0xF);
                w.Write(h1);
                w.Write(h2);
            }
            w.WriteLine();
        }

        private static byte[] ReadHexBytes(string s, ref int pos, int expectedSize)
        {
            byte[] result = new byte[expectedSize];

            for (int i = 0; i < expectedSize; i++)
            {
                if (pos + 1 >= s.Length) throw new Exception("Unexpected end of archive while reading hex data.");
                char c1 = s[pos++];
                char c2 = s[pos++];
                int n1 = HexToNibble(c1);
                int n2 = HexToNibble(c2);
                result[i] = (byte)((n1 << 4) | n2);
            }
            // consume newline(s) after hex block
            SkipNewLines(s, ref pos);
            return result;
        }

        // ---------------- manual file-format parsing helpers (no Split/IndexOf) -----------
        private static void Expect(string s, ref int pos, string token)
        {
            SkipNewLines(s, ref pos);
            for (int i = 0; i < token.Length; i++)
            {
                if (pos + i >= s.Length || s[pos + i] != token[i])
                    throw new Exception("Archive format error: expected token " + token);
            }
            pos += token.Length;
            // consume newline after token if present
            SkipNewLines(s, ref pos);
        }

        private static string ReadLine(string s, ref int pos)
        {
            StringBuilder sb = new StringBuilder();
            while (pos < s.Length && s[pos] != '\n' && s[pos] != '\r')
            {
                sb.Append(s[pos]);
                pos++;
            }
            SkipNewLines(s, ref pos);
            return sb.ToString();
        }

        private static int ReadIntLine(string s, ref int pos)
        {
            string line = ReadLine(s, ref pos);
            return ManualParseInt(line);
        }

        private static string ReadBlock(string s, ref int pos, int len)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                if (pos >= s.Length) break;
                sb.Append(s[pos++]);
            }
            // ensure we are after the block; consume newline(s)
            SkipNewLines(s, ref pos);
            return sb.ToString();
        }

        private static void SkipNewLines(string s, ref int pos)
        {
            while (pos < s.Length && (s[pos] == '\n' || s[pos] == '\r'))
                pos++;
        }

        private static bool LookAhead(string s, int pos, string token)
        {
            for (int i = 0; i < token.Length; i++)
            {
                if (pos + i >= s.Length) return false;
                if (s[pos + i] != token[i]) return false;
            }
            return true;
        }

        // ---------------- small utilities -------------------
        private static char NibbleToHex(int v)
        {
            if (v < 10) return (char)('0' + v);
            return (char)('A' + (v - 10));
        }

        private static int HexToNibble(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return 10 + (c - 'A');
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            return 0;
        }

        private static int ManualParseInt(string s)
        {
            if (s == null) return 0;
            int x = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return 0;
                x = x * 10 + (c - '0');
            }
            return x;
        }

        private static string ManualIntToString(int x)
        {
            if (x == 0) return "0";
            char[] buf = new char[20];
            int p = 0;
            int v = x;
            while (v > 0)
            {
                buf[p++] = (char)('0' + (v % 10));
                v /= 10;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = p - 1; i >= 0; i--) sb.Append(buf[i]);
            return sb.ToString();
        }

        private static bool EqualsIgnoreCase(string a, string b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                char c1 = a[i];
                char c2 = b[i];
                if (c1 >= 'A' && c1 <= 'Z') c1 = (char)(c1 + 32);
                if (c2 >= 'A' && c2 <= 'Z') c2 = (char)(c2 + 32);
                if (c1 != c2) return false;
            }
            return true;
        }
    }
}
