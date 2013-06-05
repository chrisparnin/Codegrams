using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Analysis.Counting;
using Codegrams.Persistance.Contracts;

namespace Codegrams.Analysis
{
    class UnicodeEncoder
    {
        public delegate int WordIdLookup(string key);

        public static string GetUnicodeKeyFromString(string phrase, WordIdLookup lookup)
        {
            var words = (CodegramCounter.Identifiers(phrase)
                .SelectMany(id => CodegramCounter.Words(id))
                .Select(w => lookup(w)));

            if( words.Any( w => w == -1 ) )
                return null;

            var chars = new List<char>();
            foreach (var word in words)
            {
                // if more words than range, emit extra character to cover difference.
                var offset = word;
                while (offset >= 0)
                {
                    var ch = (char)Math.Min(char.MaxValue, offset);

                    // If ch happens to fall in suggorate pair range, need proper leading and trailing surrogate pairs.
                    if (ch >= '\uD800' && ch <= '\uDBFF')
                    {
                        chars.Add(ch);
                        chars.Add('\uDFFF');
                    }
                    else if (ch >= '\uDC00' && ch <= '\uDFFF')
                    {
                        chars.Add('\uD800');
                        chars.Add(ch);
                    }
                    else
                    {
                        chars.Add(ch);
                    }
                    offset = offset - char.MaxValue;
                }
            }
            return string.Join("", chars);
        }
    }
}
