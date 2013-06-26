using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Models.Diffs
{
    public class Commit
    {
        public string UnifiedDiff {get;set;}
        public DateTime Timestamp { get; set; }
        public DiffInfo DiffInfo { get; set; }
    }
}
