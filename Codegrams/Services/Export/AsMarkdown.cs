using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Grams;

namespace Codegrams.Services.Export
{
    public class AsMarkdown
    {
        public string Export(Codegram gram)
        {
            StringWriter w = new StringWriter();
            w.WriteLine("## Codegram");
            w.WriteLine("{0} committed on {1}:", gram.Author, gram.Date);
            w.WriteLine("> {0}", gram.Message.Replace("\n","\n> "));
            w.WriteLine();
            w.WriteLine("[Full Details]({0})", gram.CommitUrl);
            foreach (var file in gram.Filegrams)
            {
                w.WriteLine("### {0}", file.FileName);

                foreach (var line in file.Linegrams)
                {
                    w.Write("     {1}", line.LineNumber, line.Line);
                }
            }
            return w.ToString();
        }

        public void ExportToFile(Codegram gram, string outputPath)
        {
            using (TextWriter writer = File.CreateText(outputPath))
            {
                writer.Write(Export(gram));
            }
        }
    }
}
