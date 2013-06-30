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
        const string REM_PREFIX = "- ";
        const string ADD_PREFIX = "+ ";

        private IEnumerable<Difference> DifferenceFromHunk(List<HunkRangeInfo> hunks)
        {
            foreach (var hunk in hunks)
            {
                int start = 0;

                int leftStart = 0;
                int rightStart = 0;
                int leftLineOffset = 0;
                int rightLineOffset = 0;

                string state = "NEW";
                var lines = hunk.DiffLines.AsEnumerable();


                while (state != "DONE")
                {
                    var line = lines.FirstOrDefault();
                    bool push = false;

                    if (state == "NEW")
                    {
                        if (line == null)
                        {
                            state = "DONE";
                        }
                        else if (line.StartsWith(REM_PREFIX) || line.StartsWith("+ "))
                        {
                            state = "DIFF_START";
                        }
                        else
                        {
                            push = true;
                            state = "NEW";
                        }
                    }

                    else if (state == "DIFF_START")
                    {
                        if (line.StartsWith(REM_PREFIX))
                        {
                            leftStart = leftLineOffset;
                            state = "DIFF_MINUS";
                        }
                        else if (line.StartsWith(ADD_PREFIX))
                        {
                            leftStart = leftLineOffset;
                            rightStart = rightLineOffset;
                            state = "DIFF_PLUS";
                        }
                        lines = IncrementLineState(lines, ref leftLineOffset, ref rightLineOffset);
                    }
                    else if (state == "DIFF_MINUS")
                    {
                        if (line.StartsWith(REM_PREFIX))
                        {
                            state = "DIFF_MINUS";
                            push = true;
                        }
                        else if (line.StartsWith(ADD_PREFIX))
                        {
                            rightStart = rightLineOffset;
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
                        if (line.StartsWith(ADD_PREFIX))
                        {
                            state = "DIFF_PLUS";
                            push = true;
                        }
                        else
                        {
                            // EMIT ADD
                            state = "NEW";
                        }
                    }
                    else if (state == "DIFF_MOD")
                    {
                        if (line.StartsWith(ADD_PREFIX))
                        {
                            state = "DIFF_PLUS";
                            push = true;
                        }
                        else
                        {
                            // EMIT MOD
                            state = "NEW";
                        }
                    }

                    //// Push onto stack
                    if (push)
                    {
                        lines = IncrementLineState(lines, ref leftLineOffset, ref rightLineOffset);
                    }
                }
            }
        }

        private IEnumerable<string> IncrementLineState(IEnumerable<string> lines, ref int leftLineOffset, ref int rightLineOffset)
        {
            var line = lines.Take(1).SingleOrDefault();
            if (line != null)
            {
                if (line.StartsWith(REM_PREFIX))
                {
                    leftLineOffset++;
                }
                else if (line.StartsWith("+ "))
                {
                    rightLineOffset++;
                }
                else
                {
                    leftLineOffset++;
                    rightLineOffset++;
                }
            }

            return lines.Skip(1);
        }

        private Difference EmitNew(int start, int length, HunkRangeInfo hunk)
        {
            return new Difference()
            {
                DifferenceType = DifferenceType.Add,
                Left = new Span()
                {
                    Start = hunk.NewHunkRange.StartingLineNumber
                },
                Right = new Span()
                {
                    Start = start,
                    Length = length
                }
            };
        }

        private Difference EmitChange(int start, int length, HunkRangeInfo hunk)
        {
            return new Difference()
            {
                DifferenceType = DifferenceType.Change,
                Left = new Span()
                {
                    Start = hunk.NewHunkRange.StartingLineNumber
                },
                Right = new Span()
                {
                    Start = start,
                    Length = length
                }
            };
        }

        private Difference EmitRemove(int start, int length, HunkRangeInfo hunk)
        {
            return new Difference()
            {
                DifferenceType = DifferenceType.Remove,
                Left = new Span()
                {
                    Start = hunk.NewHunkRange.StartingLineNumber
                },
                Right = new Span()
                {
                    Start = start,
                    Length = length
                }
            };
        }

    }
}
