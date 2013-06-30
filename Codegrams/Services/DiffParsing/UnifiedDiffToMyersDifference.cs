using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Models.Diffs;

namespace Codegrams.Services.DiffParsing
{
    public class UnifiedDiffToMyersDifference
    {
        private IEnumerable<Difference> DifferenceFromHunk(List<HunkRangeInfo> hunks)
        {
            foreach (var hunk in hunks)
            {
                bool started = false;
                int start = 0;
                int length = 0;
                int lineOffset = 0;

                string state = "NEW";
                var lines = hunk.DiffLines.AsEnumerable();
                while (state != "DONE")
                {
                    var line = lines.FirstOrDefault();

                    if (state == "NEW")
                    {
                        if (line == null)
                        {
                            state = "DONE";
                        }
                        else if (line.StartsWith("- ") || line.StartsWith("+ "))
                        {
                            state = "DIFF_START";
                        }
                        else
                        {
                            lines = lines.Skip(1);
                            state = "NEW";
                        }
                    }

                    else if (state == "DIFF_START")
                    {
                        if (line.StartsWith("- "))
                        {
                            start = lineOffset + hunk.OriginalHunkRange.StartingLineNumber;
                            state = "DIFF_MINUS";
                        }
                        else if (line.StartsWith("+ "))
                        {
                            start = lineOffset + hunk.NewHunkRange.StartingLineNumber;
                            state = "DIFF_PLUS";
                        }
                    }
                    else if (state == "DIFF_MINUS")
                    {
                        if (line.StartsWith("- "))
                        {
                            state = "DIFF_MINUS";

                        }
                        else if (line.StartsWith("+ "))
                        {
                            state = "DIFF_MOD";
                        }
                        else
                        {
                            // EMIT DEL
                            state = "NEW";
                        }
                    }
                    else if (state == "DIFF_PLUS")
                    {
                        if (line.StartsWith("+ "))
                        {
                            state = "DIFF_PLUS";
                        }
                        else
                        {
                            // EMIT ADD
                            state = "NEW";
                        }
                    }
                    else if (state == "DIFF_MOD")
                    {
                        if (line.StartsWith("+ "))
                        {
                            state = "DIFF_PLUS";
                        }
                        else
                        {
                            // EMIT MOD
                            state = "NEW";
                        }
                    }

                }
            }
        }

    }
}
