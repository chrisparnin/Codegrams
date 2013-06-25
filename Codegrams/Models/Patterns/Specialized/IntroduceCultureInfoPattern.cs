using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Patterns.Specialized
{
    class IntroduceCultureInfoPattern : Pattern
    {
        public override Grams.Codegram Rewrite(Grams.Codegram gram)
        {
            foreach (var file in gram.Filegrams)
            {
                foreach (var line in file.Linegrams)
                {
                    // Extract inner line differences?
                    // "-"
                    // "+", CultureInfo.CurrentCulture
                }
            }
            return gram;
        }
    }
}
