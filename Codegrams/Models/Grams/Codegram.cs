using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Grams
{
    public class Codegram
    {
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

        public List<Filegram> Filegrams { get; set; }

        public string CommitUrl { get; set; }

        public void Print()
        {
            foreach (var file in Filegrams)
            {
                Console.WriteLine("File: {0}", file.FileName);
                foreach (var line in file.Linegrams)
                {
                    Console.WriteLine(line.Line);
                }
            }
        }
    }
}
