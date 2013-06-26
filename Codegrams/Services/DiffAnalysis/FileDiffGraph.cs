using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codegrams.Models.Diffs;

namespace Codegrams.Services.DiffAnalysis
{
    public class FileDiffGraph
    {
        List<FileSnapshot> m_snapshots;
        public String CurrentFullName { get; private set; }

        protected FileDiffGraph()
        {
            m_snapshots = new List<FileSnapshot>();
        }

        public IEnumerable<FileSnapshot> WalkBackwards()
        {
            for (int i = m_snapshots.Count - 1; i >= 0; i--)
            {
                yield return m_snapshots[i];
            }
        }

        public FileSnapshot Last()
        {
            return m_snapshots.LastOrDefault();
        }

        public List<FileSnapshot> Snapshots { get { return m_snapshots; } }


        public static FileDiffGraph GetFileDiffGraph(IEnumerable<FileDiff> fileDiffs)
        {
            var ordered = fileDiffs.OrderBy(f => f.ParentCommit.Timestamp);
            if (ordered.Count() == 0)
                return null;

            FileDiffGraph graph = new FileDiffGraph();
            graph.CurrentFullName = ordered.First().FileName;
            var filesnapshots = fileDiffs.Select(file => new FileSnapshot(file.FileName)
            {
                LeftTextLines = file.BeforeTextLines.ToList(),
                RightTextLines = file.AfterTextLines.ToList(),
                LeftText = file.BeforeText,
                RightText = file.AfterText,

                Differences = file.Hunks.Select(hunk => new Difference()
                {
                    Left = new Span() { Start = hunk.OriginalHunkRange.StartingLineNumber, Length = hunk.OriginalHunkRange.NumberOfLines },
                    Right = new Span() { Start = hunk.NewHunkRange.StartingLineNumber, Length = hunk.NewHunkRange.NumberOfLines },
                    DifferenceType = hunk.IsAddition ? DifferenceType.Add : (hunk.IsDeletion ? DifferenceType.Remove : DifferenceType.Change)
                }).ToList()

            }).ToList();
            graph.m_snapshots = filesnapshots;
            foreach (var snapshot in graph.m_snapshots)
            {
                snapshot.BuildLineMaps();
                snapshot.ForwardChains = new List<DiffChain>();
                snapshot.BackChains = new List<DiffChain>();
            }
            graph.Chain();

            return graph;
        }

        protected void Chain()
        {
            var a = m_snapshots.FirstOrDefault();
            if (a == null)
                return;

            foreach (var next in m_snapshots.Skip(1))
            {
                for (int right = 0; right < a.RightTextLines.Count; right++)
                {
                    var forwardChain = DiffChain.ConstructForwardDiffChain(a, next, right);
                    a.ForwardChains.Add(forwardChain);
                }

                for (int right = 0; right < next.RightTextLines.Count; right++)
                {
                    var backChain = DiffChain.ConstructBackwardDiffChain(a, next, right);
                    next.BackChains.Add(backChain);
                }
                a = next;
            }

            // Verify
            var prevRightSideCount = m_snapshots.First().RightTextLines.Count;
            var prevForwardChainCount = m_snapshots.First().ForwardChains.Count;
            a = m_snapshots.First();
            foreach (var next in m_snapshots.Skip(1))
            {
                //int backChainCount = next.BackChains.Where( chain => chain.Kind != DiffChainType.Null && chain.End != null ).Count();
                //if (backChainCount != prevRightSideCount)
                //{
                //    throw new Exception("Invalid State: " + backChainCount + " != " + prevRightSideCount);
                //}

                //int count = next.Differences.Where( d => d.DifferenceType == DifferenceType.Add ).Select( d => d.Right.Length ).Sum();
                //if (prevForwardChainCount != next.RightTextLines.Count - count )
                //{
                //    throw new Exception("Invalid State: " + prevForwardChainCount + " != " + next.RightTextLines.Count);
                //}
                //a = next;
                //prevRightSideCount = next.RightTextLines.Count;
                //prevForwardChainCount = next.ForwardChains.Count;
            }
        }



    }
}
