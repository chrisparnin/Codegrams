using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Analysis.Counting;
using Codegrams.Persistance.Contracts;
using Ionic.Zip;

namespace Codegrams.Analysis
{
    public class CodegramBuilder
    {
        public CodegramBuilder(params string[] extensions)
        {
            this.Extensions = extensions;
        }
        public string[] Extensions { get; set; }

        public CodegramsContract Build(string projectsDir)
        {
            var codegrams = new CodegramsContract();
            codegrams.SequenceFrequency = new Dictionary<string, SequenceFrequency>();
            codegrams.KeyFrequency = new Dictionary<string, KeyFrequency>();

            codegrams.WordId = new Dictionary<string, int>();

            var projects = Directory.GetFiles(projectsDir).Where(p => p.EndsWith(".zip")).ToList();

            // We are optimizing for space, so using two passes over input.
            // 1a) First Pass: Read words and identifiers
            foreach (var project in projects)
            {
                using (var stream = new ZipInputStream(project))
                {
                    // sum word counts in code file
                    foreach (var codeContent in ExtractCodeContent(stream))
                    {
                        var counter = new CodegramCounter();
                        var identSequences = counter.CountIdentifierSequences(1, codeContent);
                        var wordSequences = counter.CountWordSequences(1, codeContent);

                        foreach (var ident in identSequences.Keys)
                        {
                            if (!codegrams.KeyFrequency.ContainsKey(ident))
                            {
                                codegrams.KeyFrequency[ident] = new KeyFrequency();
                            }
                            codegrams.KeyFrequency[ident].IdentifierFrequency += identSequences[ident];
                        }

                        foreach (var word in wordSequences.Keys)
                        {
                            if (!codegrams.KeyFrequency.ContainsKey(word))
                            {
                                codegrams.KeyFrequency[word] = new KeyFrequency();
                            }
                            codegrams.KeyFrequency[word].WordFrequency += wordSequences[word];
                        }
                    }
                }
                Console.WriteLine("Counting words in project {0}", project); 
            }

            // 1b) Sort by word frequency and assign numerical word id
            int id = 0;
            foreach (var word in codegrams.KeyFrequency
                        .OrderByDescending(w => w.Value.IdentifierFrequency + w.Value.WordFrequency)
                        .Select(w => w.Key)
                    )
            {
                codegrams.WordId[word] = id++;
            }

            // 2) Second Pass: Build Codegrams
            foreach (var project in projects)
            {
                using (var stream = new ZipInputStream(project))
                {
                    // sum word and identifier gram counts in code file
                    foreach (var codeContent in ExtractCodeContent(stream))
                    {
                        var counter = new CodegramCounter();
                        var identifiersGrams = counter.CountIdentifierSequences(2, codeContent);
                        var wordGrams = counter.CountWordSequences(2, codeContent);

                        foreach (var phrase in wordGrams.Keys)
                        {
                            if (phrase.All(c => c == '_'))
                                continue;

                            string unicodeKey = UnicodeEncoder.GetUnicodeKeyFromString(phrase, word => codegrams.WordId[word]);
                            //CodegramCounter.IncrementKeyCountByValue(unicodeKey, codegrams.WordSequenceFrequencyMap, wordGrams[phrase]);
                            if (!codegrams.SequenceFrequency.ContainsKey(unicodeKey))
                            {
                                codegrams.SequenceFrequency[unicodeKey] = new SequenceFrequency();
                            }
                            codegrams.SequenceFrequency[unicodeKey].WordSequenceFrequency += wordGrams[phrase];
                        }

                        foreach (var phrase in identifiersGrams.Keys)
                        {
                            if (phrase.All(c => c == '_'))
                                continue;

                            string unicodeKey = UnicodeEncoder.GetUnicodeKeyFromString(phrase, word => codegrams.WordId[word]);
                            //CodegramCounter.IncrementKeyCountByValue(unicodeKey, codegrams.IdentifierSequenceFrequencyMap, identifiersGrams[phrase]);
                            if (!codegrams.SequenceFrequency.ContainsKey(unicodeKey))
                            {
                                codegrams.SequenceFrequency[unicodeKey] = new SequenceFrequency();
                            }
                            codegrams.SequenceFrequency[unicodeKey].IdentifierSequenceFrequency += identifiersGrams[phrase];
                        }
                    }
                }
            }
            return codegrams;
        }

        private IEnumerable<string> ExtractCodeContent(ZipInputStream stream)
        {
            ZipEntry e;
            while ((e = stream.GetNextEntry()) != null)
            {
                if( Extensions.Any( ext => e.FileName.ToLower().EndsWith( ext ) ) )
                {
                    Console.WriteLine(e.FileName);
                    // Do not close sr...affects stream
                    var sr = new StreamReader(stream);
                    yield return sr.ReadToEnd();
                }
            }
        }
    }
}
