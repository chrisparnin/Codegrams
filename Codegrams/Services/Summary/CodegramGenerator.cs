using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Grams;
using Codegrams.Reader;
using Codegrams.Services.DiffParsing;

namespace Codegrams.Services.Summary
{
    public class CodegramGenerator
    {
        CodegramsReader reader;
        public CodegramGenerator(string dbPath)
        {
            reader = new CodegramsReader();
            reader.Connect(dbPath);

        }
        public Codegram GenerateFromDiffFile(string diffFile, int maxSize)
        {
            return GenerateCodegramFromDiffContent(File.ReadAllText(diffFile), maxSize);
        }

        public Codegram GenerateCodegramFromDiffContent(string diffContent, int maxSize)
        {
            GitDiffParser parser = new GitDiffParser();
            var files = parser.Parse(diffContent);

            var codegram = new Codegram();
            codegram.Filegrams = new List<Filegram>();

            foreach (var file in files)
            {
                var filegram = new Filegram();
                filegram.FileName = file.First().FileName;

                foreach (var chunk in file)
                {
                    var salient = chunk.DiffLines
                    .Select(line =>
                        new
                        {
                            Salience = reader.LineSalience(2, line),
                            Line = line
                        }
                    )
                    .OrderBy(salientLine => 1 - salientLine.Salience)
                    .ToList();

                    var lines = salient
                    .Take(maxSize)
                    .Select(salientLine => new Linegram() { Line = salientLine.Line });

                    filegram.Linegrams = lines.ToList();
                }
                codegram.Filegrams.Add(filegram);
            }

            return codegram;
        }
    }
}
