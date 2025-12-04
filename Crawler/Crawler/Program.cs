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
            Console.WriteLine(" load <файл>");
            Console.WriteLine(" print");
            Console.WriteLine(" PRINT <път>");
            Console.WriteLine(" PRINTP <път>");
            Console.WriteLine(" SET <път> \"<текст>\"");
            Console.WriteLine(" COPY <източник> <цел>");
            Console.WriteLine(" SAVE <файл>");
            Console.WriteLine(" LOADA <файл>");
            Console.WriteLine(" VISUALIZE");
            Console.WriteLine(" exit\n");

            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                if (command == null) continue;

                string cmd = "";
                int k = 0;

                while (k < command.Length && command[k] != ' ')
                    cmd += command[k++];

                while (k < command.Length && command[k] == ' ') k++;

                string argument = "";
                while (k < command.Length)
                    argument += command[k++];

                if (cmd == "exit")
                {
                    Console.WriteLine("Изход...");
                    break;
                }

                else if (cmd == "load")
                {
                    if (argument == "")
                    {
                        Console.WriteLine("❗ Трябва да посочите файл!");
                        continue;
                    }

                    string path = argument;
                    if (!EndsWith(path, ".html") && !EndsWith(path, ".htm"))
                        path += ".html";

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("❌ Файлът не е намерен!");
                        continue;
                    }

                    try
                    {
                        string html = File.ReadAllText(path);
                        root = parser.Parse(html);
                        Console.WriteLine("✅ HTML зареден!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Грешка при парсване:");
                        Console.WriteLine(ex.Message);
                    }
                }

                else if (cmd == "print")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    root.Print();
                }

                else if (cmd == "SAVE")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    if (argument == "")
                    {
                        Console.WriteLine("❗ Формат: SAVE <файл>");
                        continue;
                    }

                    SimpleArchive.Save(argument, root);
                    Console.WriteLine("💾 Архивът е записа̀н!");



                    Console.WriteLine("💾 Записано!");
                }

                else if (cmd == "LOADA")
                {
                    if (argument == "")
                    {
                        Console.WriteLine("❗ Формат: LOADA <файл>");
                        continue;
                    }

                    if (!File.Exists(argument))
                    {
                        Console.WriteLine("❌ Архивът не е намерен!");
                        continue;
                    }

                    root = SimpleArchive.Load(argument);
                    Console.WriteLine("📂 Архивът е зареден!");

                }

                else if (cmd == "PRINT" || cmd == "PRINTP" || cmd == "SET" || cmd == "COPY")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    if (argument == "")
                    {
                        Console.WriteLine("❗ Липсват аргументи!");
                        continue;
                    }

                    if (cmd == "COPY")
                    {
                        string src = "";
                        string dst = "";
                        bool second = false;

                        for (int x = 0; x < argument.Length; x++)
                        {
                            char c = argument[x];
                            if (c == ' ' && !second)
                            {
                                second = true;
                                continue;
                            }

                            if (!second) src += c;
                            else dst += c;
                        }

                        src = ManualTrim(src);
                        dst = ManualTrim(dst);

                        PathSearcher search = new PathSearcher();
                        MyList<HtmlNode> sources = search.Find(root, src);
                        MyList<HtmlNode> targets = search.Find(root, dst);

                        if (sources.Count == 0 || targets.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма възли за копиране.");
                            continue;
                        }

                        int copies = 0;
                        for (int i2 = 0; i2 < sources.Count; i2++)
                        {
                            HtmlNode cp = sources[i2].ShallowCopy();

                            for (int j2 = 0; j2 < targets.Count; j2++)
                            {
                                targets[j2].AddChild(cp);
                                copies++;
                            }
                        }

                        Console.WriteLine("✔ Копирани: " + copies);
                        continue;
                    }

                    string path = "";
                    string value = "";
                    bool foundQuotes = false;
                    bool inQuotes = false;

                    for (int x = 0; x < argument.Length; x++)
                    {
                        char c = argument[x];

                        if (c == '"')
                        {
                            if (!inQuotes)
                            {
                                inQuotes = true;
                                foundQuotes = true;
                            }
                            else
                                inQuotes = false;
                        }
                        else
                        {
                            if (!foundQuotes)
                                path += c;
                            else if (inQuotes)
                                value += c;
                        }
                    }

                    path = ManualTrim(path);

                    if (cmd == "PRINT")
                    {
                        PathSearcher searcher = new PathSearcher();
                        Stopwatch sw = Stopwatch.StartNew();

                        MyList<HtmlNode> found = searcher.Find(root, path);

                        sw.Stop();

                        PrintFoundNodes(found);
                        Console.WriteLine("⏱ " + sw.ElapsedMilliseconds + " ms");
                    }

                    else if (cmd == "PRINTP")
                    {
                        PathSearcherParallel searcherP = new PathSearcherParallel();
                        Stopwatch sw = Stopwatch.StartNew();

                        MyList<HtmlNode> found = searcherP.Find(root, path);

                        sw.Stop();

                        PrintFoundNodes(found);
                        Console.WriteLine("⚡ " + sw.ElapsedMilliseconds + " ms");
                    }

                    else if (cmd == "SET")
                    {
                        if (value == "")
                        {
                            Console.WriteLine("❗ Формат: SET <път> \"<текст>\"");
                            continue;
                        }

                        PathSearcher search = new PathSearcher();
                        MyList<HtmlNode> nodes = search.Find(root, path);

                        if (nodes.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма намерени възли.");
                            continue;
                        }

                        int changed = 0;

                        for (int n = 0; n < nodes.Count; n++)
                        {
                            HtmlNode nd = nodes[n];

                            bool hasTag = false;
                            for (int c = 0; c < value.Length; c++)
                                if (value[c] == '<') hasTag = true;

                            if (!hasTag)
                            {
                                nd.InnerText = value;
                                nd.FirstChild = null;
                                changed++;
                            }
                            else
                            {
                                try
                                {
                                    HtmlParser p2 = new HtmlParser();
                                    HtmlNode frag = p2.Parse(value);

                                    nd.FirstChild = null;
                                    nd.InnerText = "";

                                    HtmlNode ch = frag.FirstChild;
                                    while (ch != null)
                                    {
                                        nd.AddChild(ch);
                                        ch = ch.NextSibling;
                                    }

                                    changed++;

                                }
                                catch
                                {
                                    Console.WriteLine("⚠ Грешка в SET HTML.");
                                }
                            }
                        }

                        Console.WriteLine("✔ Променени: " + changed);
                    }
                }

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

                if (n.FirstChild == null && n.InnerText != "")
                    Console.WriteLine(ManualTrim(n.InnerText));
                else
                    Console.WriteLine(n.ToHtmlString());
            }
        }

        static bool EndsWith(string text, string end)
        {
            if (text.Length < end.Length) return false;
            int s = text.Length - end.Length;

            for (int i = 0; i < end.Length; i++)
                if (text[s + i] != end[i])
                    return false;

            return true;
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
