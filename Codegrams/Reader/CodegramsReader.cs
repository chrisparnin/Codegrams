using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Analysis;
using Codegrams.Analysis.Counting;

namespace Codegrams.Reader
{
    public class CodegramsReader
    {
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);
        }

        private int wordCount = -1;

        public int WordCount
        {
            get
            {
                if (wordCount == -1)
                {
                    wordCount = ReadCommands.WordCount(Connection);
                }
                return wordCount;
            }
        }

        private int identifierCount = -1;
        public int IdentifierCount
        {
            get
            {
                if (identifierCount == -1)
                {
                    identifierCount = ReadCommands.IdentifierCount(Connection);
                }
                return identifierCount;
            }
        }

        private int wordSequenceCount = -1;
        public int WordSequenceCount
        {
            get
            {
                if (wordSequenceCount == -1)
                {
                    wordSequenceCount = ReadCommands.WordSequenceCount(Connection);
                }
                return wordSequenceCount;
            }
        }

        private int identifierSequenceCount = -1;
        public int IdentifierSequenceCount
        {
            get
            {
                if (identifierSequenceCount == -1)
                {
                    identifierSequenceCount = ReadCommands.IdentifierSequenceCount(Connection);
                }
                return identifierSequenceCount;
            }
        }




        public CodegramsReader()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(Environment.Is64BitProcess) // .NET 4.0
            {
                path = Path.Combine(path, "x64", "SQLite.Interop.dll" );
            }
            else
            {
                // X32
                path = Path.Combine(path, "x86", "SQLite.Interop.dll");
            }
            NativeMethods.LoadLibrary(path);
        }
        private SQLiteConnection Connection { get; set; }
        private string DBPath { get; set; }

        public void Connect(string path)
        {
            Connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            Connection.Open();
            DBPath = path;
        }

        public void Disconnect()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;
            }
        }

        public int SequenceWordFrequency(IEnumerable<string> gram)
        {
            string phrase = string.Join(".", gram);
            string key = UnicodeEncoder.GetUnicodeKeyFromString(phrase,
                word => ReadCommands.LookupWordId(Connection, word));

            if (key == null)
                return 0;

            return ReadCommands.LookupWordSequenceFrequency(Connection, key);
        }

        public int SequenceIdentifierFrequency(IEnumerable<string> gram)
        {
            string phrase = string.Join(".", gram);
            string key = UnicodeEncoder.GetUnicodeKeyFromString(phrase,
                word => ReadCommands.LookupWordId(Connection, word));

            if (key == null)
                return 0;

            return ReadCommands.LookupIdentifierSequenceFrequency(Connection, key);
        }

        public double LineSalience(int n, string line)
        {
            var counter = new CodegramCounter();
            var identifiersGrams = counter.IdentifierSequences(n, line);
            var wordGrams = counter.WordSequences(n, line);
            var words = counter.AllWords(line).Select( w => w.ToLower());
            var identifiers = counter.AllIdentifiers(line).Select( ident => ident.ToLower() );

            var sumIdentifierGrams = 0.0;
            var sumWordGrams = 0.0;
            var sumWords = 0.0;
            var sumIdentifiers = 0.0;

            foreach (var word in words)
            {
                sumWords += (ReadCommands.LookupWordFrequency(Connection, word) + 1) / (double)WordCount;
            }

            foreach (var ident in identifiers)
            {
                sumIdentifiers += (ReadCommands.LookupIdentifierFrequency(Connection, ident)+1) / (double)IdentifierCount;
            }

            foreach (var wordGram in wordGrams)
            {
                sumWordGrams += (SequenceWordFrequency(wordGram)+1) / (double)WordSequenceCount;
            }

            foreach (var identGram in identifiersGrams)
            {
                sumIdentifierGrams += (SequenceIdentifierFrequency(identGram)+1) / (double)IdentifierSequenceCount;
            }

            var vals = new double[] { sumWords, sumIdentifiers, sumWordGrams, sumIdentifierGrams };
            if ( vals.All( v => v == 0.0 ) )
            {
                return 0.0;
            }

            return vals.Where(s => s > 0.0).Min() / vals.Max();

            //return sumIdentifierGrams / IdentifierCount;
        }
    }
}
