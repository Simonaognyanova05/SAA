using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Crawler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            HtmlNode root = null;
            HtmlParser parser = new HtmlParser();

            Console.WriteLine("=== HTML Crawler ===");
            Console.WriteLine("Команди:");
            Console.WriteLine(" load \"file.html\"");
            Console.WriteLine(" print");
            Console.WriteLine(" PRINT \"<път>\"");
            Console.WriteLine(" PRINTP \"<път>\"");
            Console.WriteLine(" SET \"<път>\" \"<текст>\"");
            Console.WriteLine(" COPY \"<src>\" \"<dst>\"");
            Console.WriteLine(" SAVE \"file.saa\"");
            Console.WriteLine(" LOADA \"file.saa\"");
            Console.WriteLine(" VISUALIZE");
            Console.WriteLine(" exit\n");

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string cmd;
                string arg;
                ParseCommand(line, out cmd, out arg);

                // ================= EXIT =================
                if (cmd == "exit")
                {
                    Console.WriteLine("Изход...");
                    break;
                }

                // ================= LOAD =================
                else if (cmd == "load")
                {
                    if (arg == "")
                    {
                        Console.WriteLine("❗ Формат: load \"file.html\"");
                        continue;
                    }

                    if (!File.Exists(arg))
                    {
                        Console.WriteLine("❌ Файлът не е намерен!");
                        continue;
                    }

                    try
                    {
                        string html = File.ReadAllText(arg);
                        root = parser.Parse(html);
                        Console.WriteLine("✅ HTML зареден!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Грешка при парсване:");
                        Console.WriteLine(ex.Message);
                    }
                }

                // ================= PRINT TREE =================
                else if (cmd == "print")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }
                    root.Print();
                }

                // ================= SAVE =================
                else if (cmd == "SAVE")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    if (arg == "")
                    {
                        Console.WriteLine("❗ Формат: SAVE \"file.saa\"");
                        continue;
                    }

                    SimpleArchive.Save(arg, root);
                    Console.WriteLine("💾 Архивът е записан!");
                }

                // ================= LOADA =================
                else if (cmd == "LOADA")
                {
                    if (arg == "")
                    {
                        Console.WriteLine("❗ Формат: LOADA \"file.saa\"");
                        continue;
                    }

                    if (!File.Exists(arg))
                    {
                        Console.WriteLine("❌ Архивът не е намерен!");
                        continue;
                    }

                    root = SimpleArchive.Load(arg);
                    Console.WriteLine("📂 Архивът е зареден!");
                }

                // ================= PRINT / PRINTP / SET / COPY =================
                else if (cmd == "PRINT" || cmd == "PRINTP" || cmd == "SET" || cmd == "COPY")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    if (arg == "")
                    {
                        Console.WriteLine("❗ Липсват аргументи!");
                        continue;
                    }

                    HandleAdvanced(cmd, arg, root);
                }

                // ================= VISUALIZE =================
                else if (cmd == "VISUALIZE")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    FormHtmlRender f = new FormHtmlRender(root);
                    f.ShowDialog();
                }

                else
                {
                    Console.WriteLine("❓ Непозната команда!");
                }
            }
        }

        // =========================================================
        // Парсване: command + аргумент в кавички
        // =========================================================
        static void ParseCommand(string input, out string cmd, out string arg)
        {
            cmd = "";
            arg = "";

            int i = 0;
            while (i < input.Length && input[i] != ' ')
            {
                cmd += input[i];
                i++;
            }

            while (i < input.Length && input[i] == ' ')
                i++;

            if (i >= input.Length)
                return;

            // аргумент в кавички
            if (input[i] == '"')
            {
                i++;
                while (i < input.Length && input[i] != '"')
                {
                    arg += input[i];
                    i++;
                }
            }
            else
            {
                while (i < input.Length)
                {
                    arg += input[i];
                    i++;
                }
            }
        }

        // =========================================================
        // Advanced commands
        // =========================================================
        static void HandleAdvanced(string cmd, string arg, HtmlNode root)
        {
            if (cmd == "PRINT")
            {
                PathSearcher s = new PathSearcher();
                Stopwatch sw = Stopwatch.StartNew();
                MyList<HtmlNode> found = s.Find(root, arg);
                sw.Stop();
                PrintFoundNodes(found);
                Console.WriteLine("⏱ " + sw.ElapsedMilliseconds + " ms");
            }
            else if (cmd == "PRINTP")
            {
                PathSearcherParallel s = new PathSearcherParallel();
                Stopwatch sw = Stopwatch.StartNew();
                MyList<HtmlNode> found = s.Find(root, arg);
                sw.Stop();
                PrintFoundNodes(found);
                Console.WriteLine("⚡ " + sw.ElapsedMilliseconds + " ms");
            }
            else
            {
                Console.WriteLine("Командата е запазена без промяна.");
            }
        }

        // =========================================================
        // Helpers
        // =========================================================
        static void PrintFoundNodes(MyList<HtmlNode> found)
        {
            if (found.Count == 0)
            {
                Console.WriteLine("⚠ Няма намерени.");
                return;
            }

            Console.WriteLine("✔ Намерени: " + found.Count);

            for (int i = 0; i < found.Count; i++)
            {
                HtmlNode n = found[i];
                string text = ManualTrim(n.InnerText);
                if (text != "")
                    Console.WriteLine(text);
                else
                    Console.WriteLine(n.ToHtmlString());
            }
        }

        static string ManualTrim(string s)
        {
            int start = 0;
            while (start < s.Length && s[start] == ' ') start++;
            int end = s.Length - 1;
            while (end >= 0 && s[end] == ' ') end--;

            string res = "";
            for (int i = start; i <= end; i++)
                res += s[i];
            return res;
        }
    }
}
