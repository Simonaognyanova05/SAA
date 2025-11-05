using System;
using System.IO;

namespace Crawler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string path;

            if (args.Length == 0)
            {
                Console.Write("Въведете име на HTML файла (намира се в bin\\Debug): ");
                string fileName = Console.ReadLine();

                if (!fileName.EndsWith(".html") && !fileName.EndsWith(".htm"))
                    fileName += ".html";

                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            }
            else
            {
                path = args[0];
            }

            Console.WriteLine("\n➡ Проверка: търся файл тук:");
            Console.WriteLine(path);

            if (!File.Exists(path))
            {
                Console.WriteLine("❌ Файлът не съществува!");
                Console.WriteLine("Уверете се, че е в папката bin\\Debug.");
                Console.ReadKey();
                return;
            }

            string html = File.ReadAllText(path);
            Console.WriteLine($"\n✅ Файлът е намерен! Дължина: {html.Length} символа");

            try
            {
                HtmlParser parser = new HtmlParser();
                HtmlNode root = parser.Parse(html);
                Console.WriteLine("\n✅ Успешно изградено дърво:\n");
                root.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Грешка при парсване:");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\nНатиснете клавиш за изход...");
            Console.ReadKey();
        }
    }
}
