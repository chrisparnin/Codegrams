using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ganji_Connector.Services.LocalRepository;
using Ganji.EF.Entities.Artifacts;
using Microsoft.Ganji_Connector.Util;
using Microsoft.Ganji_Connector.Services.Diff.Providers;
using Microsoft.VisualStudio.Text.Differencing;

namespace Codegrams.Services.DiffAnalysis
{
    public class FileDiffGraph2
    {
        List<FileSnapshot2> m_snapshots;
        VSDiffProvider m_diffEngine;
        public String CurrentFullName { get; private set; }

        protected FileDiffGraph2(VSDiffProvider diffEngine)
        {
            this.m_diffEngine = diffEngine;
            m_snapshots = new List<FileSnapshot2>();
        }

        public IEnumerable<FileSnapshot2> WalkBackwards()
        {
            for (int i = m_snapshots.Count - 1; i >= 0; i--)
            {
                yield return m_snapshots[i];
            }
        }

        public FileSnapshot2 Last()
        {
            return m_snapshots.LastOrDefault();
        }

        public List<FileSnapshot2> Snapshots { get { return m_snapshots; } }


        public static FileDiffGraph2 GetFileDiffGraph(IGanjiConnectorService ganjiConnector, Document doc, IEnumerable<Commit> commits, VSDiffProvider diffEngine)
        {
            var ordered = commits.OrderBy(c => c.Timestamp);
            if (ordered.Count() == 0)
                return null;

            FileDiffGraph2 graph = new FileDiffGraph2(diffEngine);
            graph.CurrentFullName = doc.CurrentFullName;
            var filesnapshots = graph.CollectFilesnapshots(ganjiConnector, doc, ordered);
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



        protected List<FileSnapshot2> CollectFilesnapshots(IGanjiConnectorService ganjiConnector, Document doc, IEnumerable<Commit> ordered)
        {
            var leftCommit = ordered.FirstOrDefault();
            if (leftCommit == null)
                return new List<FileSnapshot2>();

            var list = new List<FileSnapshot2>();
            foreach (var commit in ordered.Skip(1))
            {
                var rightCommit = commit;

                using (var firstTmp = new TempFile())
                using (var lastTmp = new TempFile())
                {
                    var firstText = ganjiConnector.ReadCommit(leftCommit.RepositoryId, doc.CurrentFullName);
                    if (firstText == null)
                        break;
                    var firstTextLines = firstText.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None).ToList();
                    System.IO.File.WriteAllText(firstTmp.Path, firstText);

                    var lastText = ganjiConnector.ReadCommit(rightCommit.RepositoryId, doc.CurrentFullName);
                    if (lastText == null)
                        break;

                    var lastTextLines = lastText.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None).ToList();
                    System.IO.File.WriteAllText(lastTmp.Path, lastText);

                    var difflet = m_diffEngine.DiffFiles(firstTmp.Path, lastTmp.Path);
                    if (difflet.Differences.Count > 0)
                    {
                        list.Add(new FileSnapshot2(doc.CurrentFullName)
                        {
                            Difflet = difflet,
                            Differences = difflet.Differences.ToList(),
                            LeftTextLines = firstTextLines,
                            LeftText = firstText,
                            RightTextLines = lastTextLines,
                            RightText = lastText,
                            TimestampLeft = leftCommit.Timestamp,
                            TimestampRight = rightCommit.Timestamp,
                            LeftCommitId = leftCommit.Id,
                            RightCommitId = rightCommit.Id
                        });
                    }
                }

                leftCommit = rightCommit;
            }
            // Remove empty files...
            return list.Where(s => s.LeftText != "" && s.RightText != "").ToList();
        }

    }
}
