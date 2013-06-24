using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Grams;
using MarkdownSharp;

namespace Codegrams.Services.Export
{
    public class AsMarkdownHtml
    {
        public string Export(Codegram gram)
        {
            AsMarkdown formatter = new AsMarkdown();
            var markdown = formatter.Export(gram);

            var markdown2Html = new Markdown();
            var html = markdown2Html.Transform(markdown);
            return html;
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
