﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Models.Diffs
{
    public class FileDiff
    {
        public Commit ParentCommit { get; set; }

        public List<HunkRangeInfo> Hunks { get; set; }

        public string FileName { get; set; }

        public string BeforeText { get; set; }

        public string AfterText { get; set; }

        public string[] BeforeTextLines { get; set; }

        public string[] AfterTextLines { get; set; }
    }
}
