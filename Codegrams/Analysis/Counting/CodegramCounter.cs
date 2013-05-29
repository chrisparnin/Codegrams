using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codegrams.Analysis.Counting
{
    public class CodegramCounter
    {
        public Dictionary<string,int> CountIdentifierSequences(int n, string content)
        {
            var sequences = IdentifierSequences(n, content);
            return CountSequences(sequences);
        }

        public Dictionary<string, int> CountWordSequences(int n, string content)
        {
            var sequences = WordSequences(n, content);
            return CountSequences(sequences);
        }

        public IEnumerable<List<string>> IdentifierSequences(int n, string content)
        {
            var idents = AllIdentifiers(content).ToList();
            var results = new List<List<string>>();

            int idCount = 0;
            foreach (var ident in idents)
            {
                //var remaining = idents.Skip(idCount).ToList();
                //if( remaining.Count < n )
                //    yield break;
                //else
                //    yield return remaining.Take(n).ToList();
                if (idents.Count - idCount < n)
                {
                    break;
                }
                results.Add(idents.GetRange(idCount, n));
                idCount++;
            }
            return results;
        }

        public IEnumerable<List<string>> WordSequences(int n, string content)
        {
            var words = AllWords(content)
                   .Select(ident => Words(ident))
                   .SelectMany( w => w )
                   .ToList();

            var results = new List<List<string>>();
            int wCount = 0;
            foreach( var word in words )
            {
                // TODO DO this faster...
                //var remaining = words.Skip(wCount).ToList();
                //if (remaining.Count < n)
                //    yield break;
                //else
                //    yield return remaining.Take(n).ToList();
                if (words.Count - wCount < n)
                {
                    break;
                }
                results.Add(words.GetRange(wCount, n));
                wCount++;
            }
            return results;
        }

        public IEnumerable<string> AllIdentifiers(string content)
        {
            return content.Split('\n')
                // lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => Identifiers(line))
                // identifiers                                
                .SelectMany(id => id)
                .ToList();
        }

        public IEnumerable<string> AllWords(string content)
        {
            return content.Split('\n')
                // lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => Identifiers(line))
                // identifiers                                
                .SelectMany(list => list)
                .Select( id => Words(id) )
                // words
                .SelectMany( wordList => wordList)
                .ToList();
        }


        protected Dictionary<string, int> CountSequences(IEnumerable<List<string>> sequences)
        {
            var frequency = new Dictionary<string, int>();

            foreach (var seq in sequences)
            {
                string key = string.Join(".", seq);
                IncrementKeyCountByOne(key, frequency);
            }

            return frequency;
        }

        public static List<string> Identifiers(string line)
        {
            return Patterns.IdentifierSplit
                .Split(line)
                .Where(ident => !string.IsNullOrWhiteSpace(ident))
                .Where(ident => ident.All(c => char.IsLetterOrDigit(c) || c == '_'))
                //.Select( ident => ident.Replace("ß", "ss" ) )
                //.Where(ident => ident.Length > 1)
                .ToList();
        }

        public static List<string> Words(string phrase)
        {
            var allWords = new List<string>();
            foreach (var part in Patterns.UnderscoreSplit.Split(phrase))
            {
                var words = Patterns.InnerWordSplit.Split(part)
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Select(w => w.ToLower());

                allWords.AddRange(words);
            }
            return allWords;
        }

        public static void IncrementKeyCountByOne(string key, Dictionary<string, int> dictionary)
        {
            if (!dictionary.ContainsKey(key) )
            {
                dictionary[key] = 0;
            }
            dictionary[key]++;
        }

        public static void IncrementKeyCountByValue(string key, Dictionary<string, int> dictionary, int value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = 0;
            }
            dictionary[key] += value;
        }

    }
}
