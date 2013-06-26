using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codegrams.Models.Diffs;
using Roslyn.Compilers.CSharp;
using Codegrams.Extensions;

namespace Codegrams.Services.DiffAnalysis
{
    public class FileSnapshot
    {
        public FileSnapshot(string name)
        {
            this.Name = name;
            RightLineStatus = new Dictionary<int, LineStatus>();
        }

        public string Name { get; set; }
        public FileDiffGraph2 Graph { get; set; }
        public DateTime TimestampLeft { get; set; }
        public DateTime TimestampRight { get; set; }
        public List<string> RightTextLines { get; set; }
        public string RightText { get; set; }
        public List<string> LeftTextLines { get; set; }
        public string LeftText { get; set; }
        public Difflet Difflet { get; set; }
        public List<Difference> Differences { get; set; }

        public long LeftCommitId { get; set; }
        public long RightCommitId { get; set; }

        public List<DiffChain> BackChains { get; set; }
        public List<DiffChain> ForwardChains { get; set; }

        public Dictionary<int,LineStatus> RightLineStatus { get; set; }

        private Dictionary<int, int?> LeftToRightLineMap = new Dictionary<int, int?>();
        private Dictionary<int, int?> RightToLeftLineMap = new Dictionary<int, int?>();


        public int? GetLeftToRight(int index)
        {
            return LeftToRightLineMap[index];
        }

        public int? GetRightToLeft(int index)
        {
            return RightToLeftLineMap[index];
        }

        public LineStatus GetOrCreateLineStatus(int line)
        {
            if (!this.RightLineStatus.ContainsKey(line))
            {
                this.RightLineStatus[line] = new LineStatus();
            }
            return this.RightLineStatus[line];
        }


        public void BuildLineMaps()
        {
            BuildLeftToRightLineMap();
            BuildRightToLeftLineMap();
        }

        public Difference GetLeftStatus(int index)
        {
            return Differences.Where(d => index >= d.Left.Start && index < d.Left.Start + d.Left.Length).SingleOrDefault();
        }

        public Difference GetRightStatus(int index)
        {
            return Differences.Where(d => index >= d.Right.Start && index < d.Right.Start + d.Right.Length).SingleOrDefault();
        }


        private void BuildLeftToRightLineMap()
        {
            int left = 0;
            int right = 0;
            while (left < LeftTextLines.Count)
            {
                var difference = Differences.Where(d => left >= d.Left.Start && left < d.Left.Start + d.Left.Length).SingleOrDefault();
                if (difference == null)
                {
                    difference = Differences.Where(d => d.DifferenceType == DifferenceType.Add && d.Left.Start == left).SingleOrDefault();
                }

                if (difference == null)
                {
                    LeftToRightLineMap[left++] = right++;
                }
                else
                {
                    if (difference.DifferenceType == DifferenceType.Add)
                    {
                        right = difference.Right.Start + difference.Right.Length;
                        LeftToRightLineMap[left++] = right++;
                    }
                    if (difference.DifferenceType == DifferenceType.Remove)
                    {
                        for (left = difference.Left.Start; left < difference.Left.Start + difference.Left.Length; left++)
                        {
                            LeftToRightLineMap[left] = null;
                        }
                    }
                    if (difference.DifferenceType == DifferenceType.Change)
                    {
                        if (difference.Left.Length == difference.Right.Length)
                        {
                            // This is one-to-one mod.
                            for (left = difference.Left.Start; left < difference.Left.Start + difference.Left.Length; left++, right++)
                            {
                                LeftToRightLineMap[left] = right;
                            }
                        }
                        else
                        {
                            // This is really an add mixed in with changes such as line position, e.g.:
                            // 120..121
                            // var foo = 2;
                            //}  =>
                            // 120..123
                            // var foo = 2; // mod from newline
                            // 
                            // // some code
                            // }
                            var dict = ForwardMatchChangeLinesAcrossDiff(difference);
                            foreach( var entry in dict )
                            {
                                LeftToRightLineMap[entry.Key] = entry.Value;
                                left++;
                            }
                            right = difference.Right.Start + difference.Right.Length;
                        }
                    }
                }
            }

            if (LeftToRightLineMap.Count != LeftTextLines.Count)
                throw new Exception("Messed up left to right line map");
        }

        private void BuildRightToLeftLineMap()
        {
            int left = 0;
            int right = 0;
            while (right < RightTextLines.Count)
            {
                var difference = Differences.Where(d => right >= d.Right.Start && right < d.Right.Start + d.Right.Length).SingleOrDefault();
                if (difference == null)
                {
                    difference = Differences.Where(d => d.DifferenceType == DifferenceType.Remove && d.Right.Start == right).SingleOrDefault();
                }

                if (difference == null)
                {
                    RightToLeftLineMap[right++] = left++;
                }
                else
                {
                    if (difference.DifferenceType == DifferenceType.Add)
                    {
                        for (right = difference.Right.Start; right < difference.Right.Start + difference.Right.Length; right++)
                        {
                            RightToLeftLineMap[right] = null;
                        }
                    }
                    if (difference.DifferenceType == DifferenceType.Remove)
                    {
                        left = difference.Left.Start + difference.Left.Length;
                        RightToLeftLineMap[right++] = left++;
                    }
                    if (difference.DifferenceType == DifferenceType.Change)
                    {
                        if (difference.Left.Length == difference.Right.Length)
                        {
                            // This is one-to-one mod.
                            for (right = difference.Right.Start; right < difference.Right.Start + difference.Right.Length; right++, left++)
                            {
                                RightToLeftLineMap[right] = left;
                            }
                        }
                        else
                        {
                            var dict = BackwardMatchChangeLinesAcrossDiff(difference);
                            foreach (var entry in dict)
                            {
                                RightToLeftLineMap[entry.Key] = entry.Value;
                                right++;
                            }
                            left = difference.Left.Start + difference.Left.Length;
                        }
                    }
                }
            }

            if (RightToLeftLineMap.Count != RightTextLines.Count)
                throw new Exception("Messed up right to left line map");
        }

        private SyntaxTree rightAST;
        public SyntaxTree RightAST
        {
            get
            {
                if (rightAST == null)
                {
                    rightAST = SyntaxTree.ParseText(RightText);
                }
                return rightAST;
            }
        }

        private SyntaxTree leftAST;
        public SyntaxTree LeftAST
        {
            get
            {
                if (leftAST == null)
                {
                    leftAST = SyntaxTree.ParseText(LeftText);
                }
                return leftAST;
            }
        }

        public Dictionary<int,int?> ForwardMatchChangeLinesAcrossDiff(Difference difference)
        {
            Dictionary<int, int?> Matches = new Dictionary<int, int?>();
            SimilarityTool bigram = new SimilarityTool();
            var skipIndexList = new List<int>();
            for (int i = difference.Left.Start; i < difference.Left.Start + difference.Left.Length; i++)
            {
                //DiffMatchPatch.diff_match_patch m = new DiffMatchPatch.diff_match_patch();
                //var right = CollectRightDiffSide(difference);
                //var pos = m.match_main(right, this.LeftTextLines[i], 0);
                //int line = difference.Left.Start + GetLinePositionFromCharacter(pos, right);
                var scores = new List<double>();
                for (int index = difference.Right.Start; index <  difference.Right.Start + difference.Right.Length; index++)
                {
                    if (skipIndexList.Contains(index))
                    {
                        scores.Add(-1);
                    }
                    else
                    {
                        scores.Add(bigram.CompareStrings(this.LeftTextLines[i], this.RightTextLines[index]));
                    }
                }

                if (scores.Count > 0 && scores.All( s => s > -1 ))
                {
                    Matches[i] = difference.Right.Start + scores.MaxIndex();
                    skipIndexList.Add(Matches[i].Value);
                }
                else
                {
                    Matches[i] = null;
                }
            }
            var matchCount = new HashSet<int?>(Matches.Values).Where( m => m != null ).Count();
            matchCount += Matches.Values.Where( m => m == null ).Count();
            if (matchCount != difference.Left.Length)
            {
                throw new Exception("Collision in Bitap algorithm: update to exclude, previously matched lines");
            }
            return Matches;
        }

        public Dictionary<int, int?> BackwardMatchChangeLinesAcrossDiff(Difference difference)
        {
            Dictionary<int, int?> Matches = new Dictionary<int, int?>();
            SimilarityTool bigram = new SimilarityTool();
            var skipIndexList = new List<int>();
            for (int i = difference.Right.Start; i < difference.Right.Start + difference.Right.Length; i++)
            {
                // Need to rely on left to right mapping, otherwise need global search on all scores and lines.
                var entry = LeftToRightLineMap.Where(e => e.Value.HasValue && e.Value.Value == i).SingleOrDefault();
                Matches[i] = null;
                // http://stackoverflow.com/questions/1641392/the-default-for-keyvaluepair
                if (!entry.Equals(default(KeyValuePair<int,int?>)))
                {
                    Matches[i] = entry.Key;
                }
            }

            var matchCount = new HashSet<int?>(Matches.Values).Where(m => m != null).Count();
            matchCount += Matches.Values.Where(m => m == null).Count();
            if (matchCount != difference.Right.Length)
            {
                throw new Exception("Collision in Bitap algorithm: update to exclude, previously matched lines");
            }
            return Matches;
        }

        [STAThread]
        public static void Main(String[] args)
        {
            FileSnapshot snapshot = new FileSnapshot("test.cs");

            Difference d = new Difference()
            {
                Left  = new Span(){ Start = 0, Length = 2 },
                Right = new Span(){ Start = 0, Length = 4 }
            };
            snapshot.LeftTextLines = new String[]
            {
                "Difference d = new Difference(new VisualStudio.Text.Span(0,2), new VisualStudio.Text.Span(0,4), null, null );\n",
                "}"
            }.ToList();
            snapshot.RightTextLines = new String[]
            {
                "Difference d = new Difference(new VisualStudio.Text.Span(0,2), new VisualStudio.Text.Span(0,4), null, null );\n",
                "\n",
                "// Hello bob\n",
                "}"
            }.ToList();

            var dict = snapshot.ForwardMatchChangeLinesAcrossDiff(d);
            foreach( var entry in dict )
            {
                Console.WriteLine(entry.Key + ":" + entry.Value);
            }
        }
    }
}
