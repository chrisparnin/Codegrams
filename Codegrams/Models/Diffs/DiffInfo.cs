using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Models.Diffs
{
    public class DiffInfo
    {
        public DiffInfo()
        {
            Files = new List<FileDiff>();
        }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

        public List<FileDiff> Files {get;set;}
    }
}
