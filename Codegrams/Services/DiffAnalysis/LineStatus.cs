using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codegrams.Services.DiffAnalysis
{
    public class LineStatus
    {
        public bool Survived { get; set; }

        public DateTime? ModifiedNext { get; set; }


        public int DiffletFrequency { get; set; }

        public int ChurnLength { get; set; }

        public int MaxChurn { get; set; }

        public DiffChain FurthestVersion { get; set; }

        public bool Covered { get; set; }
    }
}
