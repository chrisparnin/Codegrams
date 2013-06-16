using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Patterns.Basic
{
    public class RemovedFilePattern : Pattern
    {
        public override Grams.Codegram Rewrite(Grams.Codegram gram)
        {
            // TODO: Can get more quickly from diff
            // TAG approach?? `RemovedFilePattern`, which let render choose?
            // back to the canonical tree matching problem in compilers.
            foreach( var file in gram.Filegrams )
            {
                if( file.Linegrams.All(line => line.Line.StartsWith("- ") ) )
                {
                    file.Linegrams = new List<Grams.Linegram>();
                }
            }
            return gram;
        }
    }
}
