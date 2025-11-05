using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HtmlCrawler
{
    class Program
    {
        static HtmlDocument? doc = null;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Commands: LOAD <file>, PRINT <path>, SET <path> <text>, COPY <src> <dst>, SAVEARCHIVE <file>, LOADARCHIVE <file>, VISUALIZE, EXIT");
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = CommandLineSplitter.Split(line);
                var cmd = parts.Count > 0 ? parts[0].ToUpperInvariant() : "";
                try
                {
                    if (cmd == "EXIT") break;
                    else if (cmd == "LOAD")
                    {
                        if (parts.Count < 2) { Console.WriteLine("Usage: LOAD <file>"); continue; }
                        var html = File.ReadAllText(parts[1]);
                        var parser = new HtmlParser(html);
                        doc = parser.Parse();
                        Console.WriteLine("Loaded.");
                    }
                    else if (cmd == "PRINT")
                    {
                        if (doc == null) { Console.WriteLine("No document loaded."); continue; }
                        var path = parts.Count >= 2 ? parts[1] : "//";
                        var results = QueryEngine.Evaluate(doc.Root, path);
                        foreach (var n in results) Console.WriteLine(n.GetInnerText());
                    }
                    else if (cmd == "SET")
                    {
                        if (doc == null) { Console.WriteLine("No document loaded."); continue; }
                        if (parts.Count < 3) { Console.WriteLine("Usage: SET <path> <text>"); continue; }
                        var path = parts[1];
                        var value = parts[2];
                        var nodes = QueryEngine.Evaluate(doc.Root, path);
                        Commands.SetNodes(nodes, value);
                        Console.WriteLine($"SET applied to {nodes.Count} nodes.");
                    }
                    else if (cmd == "COPY")
                    {
                        if (doc == null) { Console.WriteLine("No document loaded."); continue; }
                        if (parts.Count < 3) { Console.WriteLine("Usage: COPY <src> <dst>"); continue; }
                        var src = QueryEngine.Evaluate(doc.Root, parts[1]);
                        var dst = QueryEngine.Evaluate(doc.Root, parts[2]);
                        Commands.CopyNodes(src, dst);
                        Console.WriteLine("COPY done.");
                    }
                    else if (cmd == "SAVEARCHIVE")
                    {
                        if (doc == null) { Console.WriteLine("No document loaded."); continue; }
                        if (parts.Count < 2) { Console.WriteLine("Usage: SAVEARCHIVE <file>"); continue; }
                        ArchiveManager.SaveArchive(parts[1], doc);
                        Console.WriteLine("Archive saved.");
                    }
                    else if (cmd == "LOADARCHIVE")
                    {
                        if (parts.Count < 2) { Console.WriteLine("Usage: LOADARCHIVE <file>"); continue; }
                        doc = ArchiveManager.LoadArchive(parts[1]);
                        Console.WriteLine("Archive loaded.");
                    }
                    else if (cmd == "VISUALIZE")
                    {
                        if (doc == null) { Console.WriteLine("No document loaded."); continue; }
                        Console.WriteLine("Launching visualizer...");
                        var thread = new System.Threading.Thread(() =>
                        {
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new VisualizerForm(doc));
                        });
                        thread.SetApartmentState(System.Threading.ApartmentState.STA);
                        thread.Start();
                    }
                    else
                    {
                        Console.WriteLine("Unknown command.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
