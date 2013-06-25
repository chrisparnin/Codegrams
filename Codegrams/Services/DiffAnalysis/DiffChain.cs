using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Differencing;
using System.Diagnostics;

namespace Codegrams.Services.DiffAnalysis
{
    public enum DiffChainType
    {
        Carry,
        Transform,
        Kill,
        Null,
        Invalid
    }

    public enum DiffChainCapType
    {
        Bottom,
        Add,
        Del,
        Mod
    }

    public class DiffChain
    {
        public int? Start {get;set;}
        public int? End   {get;set;}
        public DiffChainType Kind { get; set; }
        public FileSnapshot2 StartSnap { get; set; }
        public FileSnapshot2 EndSnap { get; set; }

        public DiffChain Forward()
        {
            if (this.End == null || EndSnap.ForwardChains.Count == 0 || EndSnap.RightText == "" )
            {
                return null;
            }
            return EndSnap.ForwardChains[this.End.Value];
        }

        public DiffChain Backward()
        {
            if (this.End == null || EndSnap.BackChains.Count == 0 || EndSnap.LeftText == "" )
            {
                return null;
            }
            return EndSnap.BackChains[this.End.Value];
        }


        public static DiffChain ConstructForwardDiffChain( FileSnapshot2 a, FileSnapshot2 next, int right )
        {
            try
            {
                var chain = new DiffChain() { };
                var aStatus = chain.SelfStatusFromRight(a, right);
                var nextStatus = chain.LeftFromRightStatus(next, right);
                var chainType = chain.DetermineChainType(aStatus, nextStatus);

                chain.Kind = chainType;
                chain.Start = right;
                chain.End = next.GetLeftToRight(right);
                chain.StartSnap = a;
                chain.EndSnap = next;
                return chain;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        public static DiffChain ConstructBackwardDiffChain(FileSnapshot2 a, FileSnapshot2 next, int right)
        {
            DiffChain backward = new DiffChain();
            backward.Start = right;
            backward.StartSnap = next;
            backward.EndSnap = a;
            backward.End = null;
            int? left = next.GetRightToLeft(right); // left is a's right
            if (left == null)
            {
                backward.Kind = DiffChainType.Null;
                return backward;
            }
            //left++; // backward chaining drift?

            var aStatus = backward.RightFromLeftStatus(a, left.Value);
            var nextStatus = backward.SelfStatusFromRight(next, right);
            var chainType = backward.DetermineChainType(aStatus, nextStatus);

            backward.Kind = chainType;
            backward.End = left;
            return backward;
        }

        private DiffChainType DetermineChainType(DiffChainCapType aStatus, DiffChainCapType nextStatus)
        {
            if (aStatus == DiffChainCapType.Bottom && nextStatus == DiffChainCapType.Bottom)
            {
                return DiffChainType.Carry;
            }

            if (aStatus == DiffChainCapType.Mod && nextStatus == DiffChainCapType.Bottom)
            {
                return DiffChainType.Carry;
            }

            if (aStatus == DiffChainCapType.Bottom && nextStatus == DiffChainCapType.Mod)
            {
                return DiffChainType.Transform;
            }

            if (aStatus == DiffChainCapType.Bottom && nextStatus == DiffChainCapType.Del)
            {
                return DiffChainType.Kill;
            }

            if (aStatus == DiffChainCapType.Mod && nextStatus == DiffChainCapType.Mod)
            {
                return DiffChainType.Transform;
            }

            if (aStatus == DiffChainCapType.Mod && nextStatus == DiffChainCapType.Del)
            {
                return DiffChainType.Kill;
            }

            if (aStatus == DiffChainCapType.Del && nextStatus == DiffChainCapType.Bottom)
            {
                // Stop
                return DiffChainType.Null;
            }

            if( aStatus == DiffChainCapType.Add && nextStatus == DiffChainCapType.Bottom)
            {
                //return DiffChainType.Transform; Should be carry...?
                return DiffChainType.Carry;
            }

            if (aStatus == DiffChainCapType.Add && nextStatus == DiffChainCapType.Del)
            {
                return DiffChainType.Kill;
            }

            if (aStatus == DiffChainCapType.Add && nextStatus == DiffChainCapType.Mod)
            {
                return DiffChainType.Transform;
            }



            return DiffChainType.Invalid;
            //throw new ArgumentException("Invalidate Diff Chain State: " + aStatus + "=>" + nextStatus);
        }

        protected DiffChainCapType LeftFromRightStatus(FileSnapshot2 snapshot, int otherRight)
        {
            // Transform to local index.
            int localLeft = otherRight;
            var difference = snapshot.Differences.Where(d => localLeft >= d.Left.Start && localLeft < d.Left.Start + d.Left.Length).SingleOrDefault();

            if (difference == null)
            {
                difference = snapshot.Differences.Where(d => d.DifferenceType == DifferenceType.Add && d.Left.Start == localLeft).SingleOrDefault();
            }

            if (difference == null)
            {
                return DiffChainCapType.Bottom;
            }

            if (difference.DifferenceType == DifferenceType.Add)
            {
                // Transduce to bottom.
                //return DiffChainCapType.Bottom;
                return DiffChainCapType.Add;
            }

            if (difference.DifferenceType == DifferenceType.Change)
            {
                return DiffChainCapType.Mod;
            }

            if (difference.DifferenceType == DifferenceType.Remove)
            {
                return DiffChainCapType.Del;
            }
            throw new ArgumentException("Invalid argument to LeftToRightStatus");
        }

        protected DiffChainCapType SelfStatusFromRight(FileSnapshot2 snapshot, int right)
        {
            int localRight = right;

            var difference = snapshot.Differences.Where(d => localRight >= d.Right.Start && localRight <  d.Right.Start + d.Right.Length).SingleOrDefault();
            if (difference == null)
            {
                difference = snapshot.Differences.Where(d => d.DifferenceType == DifferenceType.Remove && d.Right.Start == localRight).SingleOrDefault();
            }

            if (difference == null)
            {
                return DiffChainCapType.Bottom;
            }

            if (difference.DifferenceType == DifferenceType.Add)
            {
                // Transduce to bottom.
                //return DiffChainCapType.Bottom;
                return DiffChainCapType.Add;
            }

            if (difference.DifferenceType == DifferenceType.Change)
            {
                return DiffChainCapType.Mod;
            }

            if (difference.DifferenceType == DifferenceType.Remove)
            {
                return DiffChainCapType.Del;
            }
            throw new ArgumentException("Invalid argument to RightToLeftStatus");
        }

        protected DiffChainCapType RightFromLeftStatus(FileSnapshot2 snapshot, int otherLeft)
        {
            // Transform to local index.
            int localRight = otherLeft;
            // TODO: Is this bounded at end?
            var difference = snapshot.Differences.Where(d => localRight >= d.Right.Start && localRight < d.Right.Start + d.Right.Length).SingleOrDefault();
            if (difference == null)
            {
                return DiffChainCapType.Bottom;
            }

            if (difference.DifferenceType == DifferenceType.Add)
            {
                // Transduce to bottom.
                //return DiffChainCapType.Bottom;
                return DiffChainCapType.Add;
            }

            if (difference.DifferenceType == DifferenceType.Change)
            {
                return DiffChainCapType.Mod;
            }

            if (difference.DifferenceType == DifferenceType.Remove)
            {
                return DiffChainCapType.Del;
            }
            throw new ArgumentException("Invalid argument to RightToLeftStatus");
        }

    }

   


}
