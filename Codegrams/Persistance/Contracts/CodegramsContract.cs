using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Persistance.Contracts
{
    [DataContract]
    public class CodegramsContract
    {
        public Dictionary<string, int> WordId { get; set; }
        public Dictionary<string, KeyFrequency> KeyFrequency { get; set; }
        public Dictionary<string, SequenceFrequency> SequenceFrequency { get; set; }
    }

    [DataContract]
    public class KeyFrequency
    {
        public int WordFrequency { get; set; }
        public int IdentifierFrequency { get; set; }
    }

    [DataContract]
    public class SequenceFrequency
    {
        public int WordSequenceFrequency { get; set; }
        public int IdentifierSequenceFrequency { get; set; }
    }
}
