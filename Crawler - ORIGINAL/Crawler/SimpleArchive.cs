using System;
using System.IO;
using System.Text;

namespace Crawler
{
    public static class SimpleArchive
    {
        public static void Save(string archivePath, HtmlNode root)
        {
            if (archivePath == null) throw new ArgumentNullException(nameof(archivePath));
            if (root == null) throw new ArgumentNullException(nameof(root));

            string html = root.ToHtmlString();
            string compressedHtml = SimpleCompressor.Compress(html);

            MyList<HtmlNode> images = CollectImageNodes(root);

            FileStream fs = null;
            StreamWriter w = null;
            try
            {
                fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
                w = new StreamWriter(fs, Encoding.UTF8);

                WriteLine(w, "[SAA]");
                WriteLine(w, "[HTML_SIZE]");
                WriteLine(w, ManualIntToString(compressedHtml.Length));
                WriteLine(w, "[HTML_COMPRESSED]");

                w.Write(compressedHtml);
                w.Write('\n');

                for (int i = 0; i < images.Count; i++)
                {
                    HtmlNode img = images[i];
                    string src = img.Attributes.Get("src");
                    if (src == null) continue;
                    if (!File.Exists(src)) continue;

                    byte[] data = File.ReadAllBytes(src);

                    WriteLine(w, "[FILE]");
                    WriteLine(w, src);
                    WriteLine(w, "[SIZE]");
                    WriteLine(w, ManualIntToString(data.Length));
                    WriteLine(w, "[DATA]");
                    WriteHex(data, w);
                }

                WriteLine(w, "[END]");
            }
            finally
            {
                if (w != null) w.Dispose();
                if (fs != null) fs.Dispose();
            }
        }

        public static HtmlNode Load(string archivePath)
        {
            if (archivePath == null) throw new ArgumentNullException(nameof(archivePath));
            if (!File.Exists(archivePath)) throw new FileNotFoundException("Archive not found", archivePath);

            string content = File.ReadAllText(archivePath, Encoding.UTF8);
            int pos = 0;

            Expect(content, ref pos, "[SAA]");

            Expect(content, ref pos, "[HTML_SIZE]");
            int htmlSize = ReadIntLine(content, ref pos);

            Expect(content, ref pos, "[HTML_COMPRESSED]");
            string compressedHtml = ReadBlock(content, ref pos, htmlSize);

            string html = SimpleCompressor.Decompress(compressedHtml);
            HtmlParser parser = new HtmlParser();
            HtmlNode root = parser.Parse(html);
           
            HtmlNode currentRoot = root;
            for (int i = 0; i < 2; i++)
            {
                if (currentRoot != null && EqualsIgnoreCase(currentRoot.TagName, "root") && currentRoot.FirstChild != null)
                {
                    currentRoot = currentRoot.FirstChild;
                }
                else
                {
                    break;
                }
            }
            root = currentRoot;
            
            while (true)
            {
                SkipNewLines(content, ref pos); 
                if (LookAhead(content, pos, "[END]"))
                {
                    Expect(content, ref pos, "[END]");
                    break;
                }

                Expect(content, ref pos, "[FILE]");
                string filename = ReadLine(content, ref pos);

                Expect(content, ref pos, "[SIZE]");
                int size = ReadIntLine(content, ref pos);

                Expect(content, ref pos, "[DATA]");
                byte[] data = ReadHexBytes(content, ref pos, size);

                File.WriteAllBytes(filename, data);
            }

            return root;
        }

        private static string ReadBlock(string s, ref int pos, int len)
        {
            string result = "";
            for (int i = 0; i < len; i++)
            {
                if (pos >= s.Length) break;
                result += s[pos++];
            }
            return result;
        }


        private static void WriteLine(StreamWriter w, string s)
        {
            w.Write(s);
            w.Write('\n');
        }

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

        private static void WriteHex(byte[] data, StreamWriter w)
        {
            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i];
                char h1 = NibbleToHex((b >> 4) & 0xF);
                char h2 = NibbleToHex(b & 0xF);
                w.Write(h1);
                w.Write(h2);
            }
            w.Write('\n');
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
            SkipNewLines(s, ref pos);
            return result;
        }

        private static void Expect(string s, ref int pos, string token)
        {
            SkipNewLines(s, ref pos);
            for (int i = 0; i < token.Length; i++)
            {
                if (pos + i >= s.Length || s[pos + i] != token[i])
                    throw new Exception("Archive format error: expected token " + token);
            }
            pos += token.Length;
            SkipNewLines(s, ref pos);
        }

        private static string ReadLine(string s, ref int pos)
        {
            string result = "";
            while (pos < s.Length && s[pos] != '\n' && s[pos] != '\r')
            {
                result += s[pos];
                pos++;
            }
            SkipNewLines(s, ref pos);
            return result;
        }

        private static int ReadIntLine(string s, ref int pos)
        {
            string line = ReadLine(s, ref pos);
            return ManualParseInt(line);
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
            string result = "";
            for (int i = p - 1; i >= 0; i--) result += buf[i];
            return result;
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