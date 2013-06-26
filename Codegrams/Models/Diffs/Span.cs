using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Models.Diffs
{
    public struct Span
    {
        public int End { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public bool IsEmpty { get; set; }
    }
}
