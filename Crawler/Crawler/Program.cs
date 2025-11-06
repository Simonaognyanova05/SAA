using System;
using System.Collections.Generic;
using System.IO;

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
            Console.WriteLine(" - PRINT <път>   → извежда резултат по релативен път (XPath)");
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
                    if (!path.EndsWith(".html") && !path.EndsWith(".htm"))
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
                else if (cmd == "PRINT")
                {
                    if (root == null)
                    {
                        Console.WriteLine("❗ Няма зареден документ!");
                        continue;
                    }

                    if (argument == "")
                    {
                        Console.WriteLine("❗ Моля, въведете път, напр. PRINT //html/body/p");
                        continue;
                    }

                    PathSearcher searcher = new PathSearcher();
                    List<HtmlNode> found = searcher.Find(root, argument);

                    if (found.Count == 0)
                    {
                        Console.WriteLine("⚠ Няма намерени елементи.");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Намерени елементи: {found.Count}");
                        foreach (var node in found)
                        {
                            if (node.FirstChild == null && !string.IsNullOrWhiteSpace(node.InnerText))
                            {
                                Console.WriteLine(node.InnerText.Trim());
                            }
                            else
                            {
                                Console.WriteLine(node.ToHtmlString());
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❓ Непозната команда: " + cmd);
                    Console.WriteLine("Опитайте: load <file>, print, PRINT <path>, exit");
                }
            }
        }
    }
}
