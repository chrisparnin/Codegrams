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
        public Codegram GenerateFromDiff(string diffFile, int maxSize, string dbPath)
        {
            GitDiffParser parser = new GitDiffParser();
            var files = parser.Parse(File.ReadAllText(diffFile));

            var reader = new CodegramsReader();
            reader.Connect(dbPath);

            var codegram = new Codegram();
            codegram.Filegrams = new List<Filegram>();

            foreach( var file in files )
            {
                var filegram = new Filegram();
                filegram.FileName = file.First().FileName;

                foreach( var chunk in file )
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
                    .Select( salientLine => new Linegram(){Line= salientLine.Line}) ;

                    filegram.Linegrams = lines.ToList();
                }
                codegram.Filegrams.Add(filegram);
            }

            reader.Disconnect();
            return codegram;
        }
    }
}
