using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Grams;
using Codegrams.Reader;
using Codegrams.Services.DiffParsing;
using Codegrams.Services.Metrics;
using Codegrams.Patterns.Basic;
using Codegrams.Patterns;

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
            var diffInfo = parser.Parse(diffContent);

            var codegram = new Codegram();
            codegram.Filegrams = new List<Filegram>();
            codegram.Author = diffInfo.Author;
            codegram.Date = diffInfo.Date;
            codegram.Message = diffInfo.Message;

            CrossSimiliarity(diffInfo.Files);

            foreach (var file in diffInfo.Files)
            {
                var filegram = new Filegram();
                filegram.FileName = file.Hunks.First().FileName;

                foreach (var chunk in file.Hunks)
                {
                    var lines = SalientLines(maxSize, chunk);

                    filegram.Linegrams = lines.ToList();
                }
                codegram.Filegrams.Add(filegram);
            }

            // Temp placement: Apply patterns
            Pattern pattern = new RemovedFilePattern();
            pattern.Rewrite(codegram);

            return codegram;
        }

        private Dictionary<string, int> LineFrequency(List<string> allLines)
        {
            var freq = new Dictionary<string, int>();
            foreach (var line in allLines)
            {
                if (!freq.ContainsKey(line))
                    freq[line] = 0;
                freq[line]++;
            }
            return freq;
        }

        private void CrossSimiliarity(IEnumerable<FileDiff> files)
        {
            var allLines = new List<string>();
            foreach (var file in files)
            {
                foreach (var chunk in file.Hunks)
                {
                    allLines.AddRange(chunk.DiffLines);
                }
            }

            var freq = LineFrequency(allLines);

            allLines = allLines.Distinct().ToList();

            var crossSimiliarity = new double[allLines.Count][];
            int i = 0;
            foreach (var line in allLines)
            {
                int j = 0;
                crossSimiliarity[i] = new double[allLines.Count];
                foreach (var otherLine in allLines)
                {
                    double val = 0.0;
                    if (i != j)
                    {
                        val = LineSimiliarity.CompareStrings(line, otherLine);
                        if (double.IsNaN(val))
                        {
                            val = 0.0;
                        }
                    }
                    crossSimiliarity[i][j] = val * freq[line];
                    j++;
                }
                i++;
            }
            var sum = crossSimiliarity.Sum(row => row.Sum())/ (allLines.Count);
            var sumFreq = freq.Sum( pair => pair.Value - 1 );
            Console.WriteLine("line freq sum {0}", sumFreq);
            Console.WriteLine("cross-sim sum {0}", sum);
        }

        private IEnumerable<Linegram> SalientLines(int maxSize, HunkRangeInfo chunk)
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
            return lines;
        }
    }
}
