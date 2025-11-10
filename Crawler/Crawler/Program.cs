using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Crawler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            HtmlNode root = null;
            HtmlParser parser = new HtmlParser();

            Console.WriteLine("=== HTML Crawler ===");
            Console.WriteLine("Налични команди:");
            Console.WriteLine(" - load <файл>   → зарежда HTML файл");
            Console.WriteLine(" - print         → показва дървото");
            Console.WriteLine(" - PRINT <път>   → търсене по път (нормално)");
            Console.WriteLine(" - PRINTP <път>  → паралелно търсене");
            Console.WriteLine(" - SET <път> \"<ново съдържание>\" → промяна на възел");
            Console.WriteLine(" - COPY <източник> <цел> → копиране на възел");
            Console.WriteLine(" - exit          → изход\n");

            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                if (command == null) continue;

                string cmd = "";
                int i = 0;

                while (i < command.Length && command[i] != ' ')
                {
                    cmd += command[i];
                    i++;
                }

                while (i < command.Length && command[i] == ' ') i++;

                string argument = "";
                while (i < command.Length)
                {
                    argument += command[i];
                    i++;
                }

                if (cmd == "exit")
                {
                    Console.WriteLine("👋 Изход от програмата...");
                    break;
                }
                else if (cmd == "load")
                {
                    if (argument == "")
                    {
                        Console.WriteLine("❗ Трябва да посочите име на файл!");
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

                    string html = File.ReadAllText(path);
                    try
                    {
                        root = parser.Parse(html);
                        Console.WriteLine("✅ Файлът е успешно зареден и парсиран!");
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
                    }
                    else
                    {
                        Console.WriteLine("\n=== Дървовиден модел на HTML ===");
                        root.Print();
                        Console.WriteLine("================================\n");
                    }
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
                        Console.WriteLine("❗ Моля, въведете аргументи!");
                        continue;
                    }

                    // --- Копиране ---
                    if (cmd == "COPY")
                    {
                        // Разделяме аргументите ръчно (без Split)
                        string srcPath = "";
                        string dstPath = "";
                        bool second = false;

                        for (int j = 0; j < argument.Length; j++)
                        {
                            char c = argument[j];
                            if (c == ' ' && !second)
                            {
                                second = true;
                            }
                            else
                            {
                                if (!second) srcPath += c;
                                else dstPath += c;
                            }
                        }

                        srcPath = ManualTrim(srcPath);
                        dstPath = ManualTrim(dstPath);

                        if (srcPath == "" || dstPath == "")
                        {
                            Console.WriteLine("❗ Формат: COPY <източник> <цел>");
                            continue;
                        }

                        PathSearcher searcher = new PathSearcher();
                        List<HtmlNode> sources = searcher.Find(root, srcPath);
                        List<HtmlNode> targets = searcher.Find(root, dstPath);

                        if (sources.Count == 0 || targets.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма намерени възли за копиране.");
                            continue;
                        }

                        int copies = 0;
                        foreach (var src in sources)
                        {
                            HtmlNode copy = src.ShallowCopy(); // плитко копие
                            foreach (var tgt in targets)
                            {
                                tgt.AddChild(copy);
                                copies++;
                            }
                        }

                        Console.WriteLine($"✅ Копирани възли: {copies}");
                        continue;
                    }

                    // --- SET / PRINT / PRINTP ---
                    string path = "";
                    string value = "";
                    bool inQuotes = false;
                    bool foundQuote = false;

                    for (int j = 0; j < argument.Length; j++)
                    {
                        char c = argument[j];

                        if (c == '"')
                        {
                            if (!inQuotes)
                            {
                                inQuotes = true;
                                foundQuote = true;
                            }
                            else
                            {
                                inQuotes = false;
                            }
                        }
                        else
                        {
                            if (!foundQuote)
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
                        List<HtmlNode> found = searcher.Find(root, path);
                        sw.Stop();

                        if (found.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма намерени елементи.");
                        }
                        else
                        {
                            Console.WriteLine($"✅ Намерени елементи: {found.Count}");
                            foreach (var node in found)
                            {
                                if (node.FirstChild == null && node.InnerText != "")
                                    Console.WriteLine(node.InnerText.Trim());
                                else
                                    Console.WriteLine(node.ToHtmlString());
                            }
                        }
                        Console.WriteLine($"⏱ Време: {sw.ElapsedMilliseconds} ms\n");
                    }
                    else if (cmd == "PRINTP")
                    {
                        PathSearcherParallel searcher = new PathSearcherParallel();
                        Stopwatch sw = Stopwatch.StartNew();
                        List<HtmlNode> found = searcher.Find(root, path);
                        sw.Stop();

                        if (found.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма намерени елементи.");
                        }
                        else
                        {
                            Console.WriteLine($"✅ Намерени елементи (паралелно): {found.Count}");
                            foreach (var node in found)
                            {
                                if (node.FirstChild == null && node.InnerText != "")
                                    Console.WriteLine(node.InnerText.Trim());
                                else
                                    Console.WriteLine(node.ToHtmlString());
                            }
                        }
                        Console.WriteLine($"⚡ Паралелно време: {sw.ElapsedMilliseconds} ms\n");
                    }
                    else if (cmd == "SET")
                    {
                        if (value == "")
                        {
                            Console.WriteLine("❗ Формат: SET <път> \"<ново съдържание>\"");
                            continue;
                        }

                        PathSearcher searcher = new PathSearcher();
                        List<HtmlNode> nodes = searcher.Find(root, path);

                        if (nodes.Count == 0)
                        {
                            Console.WriteLine("⚠ Няма намерени елементи по зададения път.");
                            continue;
                        }

                        int changed = 0;
                        HtmlParser innerParser = new HtmlParser();

                        for (int n = 0; n < nodes.Count; n++)
                        {
                            HtmlNode node = nodes[n];
                            bool hasTag = false;
                            for (int c = 0; c < value.Length; c++)
                            {
                                if (value[c] == '<') { hasTag = true; break; }
                            }

                            if (hasTag)
                            {
                                try
                                {
                                    HtmlNode frag = innerParser.Parse(value);
                                    node.FirstChild = null;
                                    node.InnerText = "";
                                    HtmlNode ch = frag.FirstChild;
                                    while (ch != null)
                                    {
                                        node.AddChild(ch);
                                        ch = ch.NextSibling;
                                    }
                                    changed++;
                                }
                                catch
                                {
                                    Console.WriteLine($"⚠ Грешка при парсване на HTML за {node.TagName}");
                                }
                            }
                            else
                            {
                                node.InnerText = value;
                                node.FirstChild = null;
                                changed++;
                            }
                        }

                        Console.WriteLine($"✅ Променени възли: {changed}");
                    }
                }
                else
                {
                    Console.WriteLine("❓ Непозната команда: " + cmd);
                }
            }
        }

        static bool EndsWith(string text, string end)
        {
            if (text.Length < end.Length) return false;
            int start = text.Length - end.Length;
            for (int i = 0; i < end.Length; i++)
            {
                if (text[start + i] != end[i])
                    return false;
            }
            return true;
        }

        static string ManualTrim(string input)
        {
            int start = 0;
            while (start < input.Length && input[start] == ' ') start++;

            int end = input.Length - 1;
            while (end >= 0 && input[end] == ' ') end--;

            string res = "";
            for (int i = start; i <= end && i < input.Length; i++)
                res += input[i];
            return res;
        }
    }
}
