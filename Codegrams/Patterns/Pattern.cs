using Codegrams.Grams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Patterns
{
    public abstract class Pattern
    {
        public abstract Codegram Rewrite(Codegram gram);
    }
}
