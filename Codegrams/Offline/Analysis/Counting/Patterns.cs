using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codegrams.Analysis.Counting
{
    public class Patterns
    {
        public static Regex InnerWordSplit = new Regex("(?<!(^|[A-Z]))(?=[A-Z])|(?<!^)(?=[A-Z][a-z])", RegexOptions.Compiled);
        public static Regex UnderscoreSplit = new Regex("_+", RegexOptions.Compiled);
        public static Regex IdentifierSplit = new Regex(@"(\W|\d)+", RegexOptions.Compiled);
    }
}
