using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Grams
{
    public class Filegram
    {
        public string FileName { get; set; }
        public List<Linegram> Linegrams { get; set; }
    }
}
