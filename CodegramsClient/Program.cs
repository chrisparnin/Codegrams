using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codegrams.Analysis;
using Codegrams.Collection;
using Codegrams.Collection.RepoSource;
using Codegrams.Persistance;
using Codegrams.Reader;
using Codegrams.Services.DiffParsing;
using Codegrams.Services.Summary;
using CodegramsClient.Data;

namespace CodegramsClient
{
    class Program
    {
        public static void Run()
        {
            bool download = false;
            bool analyze = false;
            if (download)
            {
                // 1) Get Known list of C# projects
                IRepositorySource source = ProjectSources.Github;
                var projects = System.IO.File.ReadAllText(DataSources.CSharpProjectsListingsPath)
                    .Split('\n')
                    .Select(p => p.Replace("\"", ""));

                // 2) Throlled/Restartable Download.
                var list = new List<string>();
                for (int i = 0; i < 10; i++)
                {
                    list.Add(projects.GetRandomElement());
                }
                BuildRepository.DownloadProjects(source, "projects", list);
            }

            if (analyze)
            {
                // 3) Analyze project zips to count code grams
                CodegramBuilder builder = new CodegramBuilder(".cs", ".xaml");
                var codegrams = builder.Build("projects");

                // 4) Save codegrams to DB.
                CodegramsDB.Init(codegrams, "codegrams.db");
            }

            // 5) Test queries.
            var reader = new CodegramsReader();
            reader.Connect("codegrams.db");

            Console.WriteLine(reader.SequenceIdentifierFrequency(new string[] { "using", "System" }));
            Console.WriteLine(reader.SequenceWordFrequency(new string[] { "using", "System" }));

            Console.WriteLine(reader.LineSalience(2, "using System"));
            Console.WriteLine(reader.LineSalience(2, "Console.WriteLine"));
            Console.WriteLine(reader.LineSalience(2, "CodegramsDB.Init"));
            Console.WriteLine(reader.LineSalience(2, "using Codegrams.Persistance"));

            var lines =
            File.ReadAllText(@"C:\DEV\github\Codegrams\CodegramsClient\Program.cs")
                .Split('\n')
                .Select(line =>
                    new
                    {
                        Salience = reader.LineSalience(2, line),
                        Line = line
                    }
                ).OrderBy(salientLine => 1 - salientLine.Salience);

            foreach (var line in lines)
            {
                Console.WriteLine(line.Salience + ":" + line.Line);
            }
        }

        static void Main(string[] args)
        {
            //Run();
            PrintDiff(@"C:\data\commits\test.diff");
            PrintDiff(@"C:\data\commits\test2.diff");
            PrintDiff(@"C:\data\commits\test3.diff");
            PrintDiff(@"C:\data\commits\test4.diff");

            var generator = new CodegramGenerator( "codegrams.db");

            var codegram = generator.GenerateFromDiffFile(@"C:\data\commits\test4.diff", 3);
            codegram.Print();

            codegram = generator.GenerateFromDiffFile(@"C:\data\commits\test3.diff", 3);
            codegram.Print();

            codegram = generator.GenerateFromDiffFile(@"C:\data\commits\test2.diff", 3);
            codegram.Print();

            codegram = generator.GenerateFromDiffFile(@"C:\data\commits\test.diff", 3);
            codegram.Print();

            Console.ReadKey();
        }

        static void PrintDiff(string diffFile)
        {
            GitDiffParser parser = new GitDiffParser();

            var chunks = parser.Parse(File.ReadAllText(diffFile));
            foreach (var file in chunks.Files)
            {
                foreach (var hunk in file.Hunks)
                {
                    Console.WriteLine(string.Format("{0} {1} #{2} {3}",
                            hunk.FileName,
                            hunk.NewHunkRange.StartingLineNumber,
                            hunk.NewHunkRange.NumberOfLines,
                            hunk.IsModification
                        ));
                }
            }
        }
    }
}
