using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codegrams.Collection.RepoSource
{
    public class Github : IRepositorySource
    {
        public bool DownloadZip(string url, string output)
        {
            string query = url.Replace("https://api.github.com/repos/","http://github.com/") + "/zipball/master";

            if (!File.Exists(output))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(query, output);
                        return true;
                    }
                }
                catch (Exception ex) { Trace.TraceError(ex.Message); }
            }
            return false;
        }
    }
}