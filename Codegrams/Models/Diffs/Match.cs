using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Models.Diffs
{
    public class Match
    {
        public Span Left { get; set; }
        public Span Right { get; set; }
        public int Length { get; set; }
    }
}
