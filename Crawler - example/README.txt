HtmlCrawler (.NET 6) - Console + Visualizer
=========================================

Contents:
- HtmlCrawler.sln
- HtmlCrawler.csproj
- Program.cs
- HtmlNode.cs
- HtmlParser.cs
- QueryEngine.cs
- Commands.cs
- ArchiveManager.cs
- VisualizerForm.cs
- CommandLineSplitter.cs
- sample.html
- img_girl.bmp

Requirements:
- .NET 6 SDK installed (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Windows (WinForms uses Windows APIs)

Build & Run (command line):
1. Open a terminal and navigate to the project folder.
2. Run:
   dotnet build
   dotnet run --project HtmlCrawler.csproj

Usage (sample):
> LOAD sample.html
> PRINT //html/body/p
> SET //html/body/p "AAA"
> COPY //html/body/div/div //html/body/table[@id='table2']/tr[2]/td
> SAVEARCHIVE output_archive
> VISUALIZE

Notes:
- This is a compact educational implementation (not a full HTML spec parser).
- Visualizer displays simple rendering for text, images (BMP), links and basic tables.
