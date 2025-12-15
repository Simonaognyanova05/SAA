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

            PrintHelp();

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string cmd;
                string[] argsQuoted = ParseQuotedCommand(line, out cmd);

                if (cmd == "exit")
                {
                    Console.WriteLine("Изход...");
                    break;
                }

                else if (cmd == "load")
                {
                    if (argsQuoted.Length != 1)
                    {
                        Console.WriteLine("Формат: load \"file.html\"");
                        continue;
                    }

                    string path = argsQuoted[0];

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Файлът не е намерен!");
                        continue;
                    }

                    string html = File.ReadAllText(path);
                    root = parser.Parse(html);
                    Console.WriteLine("HTML зареден!");
                }

                else if (cmd == "print")
                {
                    if (root == null)
                    {
                        Console.WriteLine("Няма зареден документ!");
                        continue;
                    }
                    root.Print();
                }

                else if (cmd == "SAVE")
                {
                    if (root == null || argsQuoted.Length != 1)
                    {
                        Console.WriteLine("Формат: SAVE \"file.saa\"");
                        continue;
                    }

                    SimpleArchive.Save(argsQuoted[0], root);
                    Console.WriteLine("Архивът е записан!");
                }

                else if (cmd == "LOADA")
                {
                    if (argsQuoted.Length != 1 || !File.Exists(argsQuoted[0]))
                    {
                        Console.WriteLine("Формат: LOADA \"file.saa\"");
                        continue;
                    }

                    root = SimpleArchive.Load(argsQuoted[0]);
                    Console.WriteLine("Архивът е зареден!");
                }

                else if (cmd == "PRINT" || cmd == "PRINTP")
                {
                    if (root == null || argsQuoted.Length != 1)
                    {
                        Console.WriteLine("Формат: " + cmd + " \"<path>\"");
                        continue;
                    }

                    Stopwatch sw = Stopwatch.StartNew();
                    MyList<HtmlNode> found =
                        (cmd == "PRINT")
                        ? new PathSearcher().Find(root, argsQuoted[0])
                        : new PathSearcherParallel().Find(root, argsQuoted[0]);

                    sw.Stop();
                    PrintFound(found);
                    Console.WriteLine(sw.ElapsedMilliseconds + " ms");
                }

                else if (cmd == "SET")
                {
                    if (root == null || argsQuoted.Length != 2)
                    {
                        Console.WriteLine("Формат: SET \"<path>\" \"<text>\"");
                        continue;
                    }

                    PathSearcher search = new PathSearcher();
                    MyList<HtmlNode> nodes = search.Find(root, argsQuoted[0]);

                    int changed = 0;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        HtmlNode n = nodes[i];
                        n.InnerText = argsQuoted[1];
                        n.FirstChild = null;
                        changed++;
                    }

                    Console.WriteLine("Променени: " + changed);
                }

                else if (cmd == "COPY")
                {
                    if (root == null || argsQuoted.Length != 2)
                    {
                        Console.WriteLine("Формат: COPY \"<src>\" \"<dst>\"");
                        continue;
                    }

                    PathSearcher search = new PathSearcher();
                    MyList<HtmlNode> src = search.Find(root, argsQuoted[0]);
                    MyList<HtmlNode> dst = search.Find(root, argsQuoted[1]);

                    int copies = 0;

                    for (int i = 0; i < src.Count; i++)
                    {
                        HtmlNode cp = src[i].DeepCopy();
                        for (int j = 0; j < dst.Count; j++)
                        {
                            dst[j].AddChild(cp);
                            copies++;
                        }
                    }

                    Console.WriteLine("Копирани: " + copies);
                }

                else if (cmd == "VISUALIZE")
                {
                    if (root == null)
                    {
                        Console.WriteLine("Няма зареден документ!");
                        continue;
                    }

                    new FormHtmlRender(root).ShowDialog();
                }

                else
                {
                    Console.WriteLine("Непозната команда!");
                }
            }
        }
        static string[] ParseQuotedCommand(string input, out string cmd)
        {
            cmd = "";
            int i = 0;

            while (i < input.Length && input[i] != ' ')
                cmd += input[i++];

            string[] result = new string[0];
            int count = 0;

            while (i < input.Length)
            {
                if (input[i] == '"')
                {
                    i++;
                    string val = "";
                    while (i < input.Length && input[i] != '"')
                        val += input[i++];

                    Array.Resize(ref result, count + 1);
                    result[count++] = val;
                }
                i++;
            }

            return result;
        }
        static void PrintFound(MyList<HtmlNode> found)
        {
            if (found.Count == 0)
            {
                Console.WriteLine("Няма намерени.");
                return;
            }

            for (int i = 0; i < found.Count; i++)
            {
                HtmlNode n = found[i];
                if (n.InnerText != "")
                    Console.WriteLine(n.InnerText.Trim());
                else
                    Console.WriteLine(n.ToHtmlString());
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("=== HTML Crawler ===");
            Console.WriteLine(" load \"file.html\"");
            Console.WriteLine(" print");
            Console.WriteLine(" PRINT \"path\"");
            Console.WriteLine(" PRINTP \"path\"");
            Console.WriteLine(" SET \"path\" \"text\"");
            Console.WriteLine(" COPY \"src\" \"dst\"");
            Console.WriteLine(" SAVE \"file.saa\"");
            Console.WriteLine(" LOADA \"file.saa\"");
            Console.WriteLine(" VISUALIZE");
            Console.WriteLine(" exit\n");
        }
    }
}
