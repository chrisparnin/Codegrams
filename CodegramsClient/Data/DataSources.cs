using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegramsClient.Data
{
    // Data based off of http://www.ghtorrent.org/ export 5/24/2013
    class DataSources
    {
        static DataSources()
        {
            CSharpProjectsListingsPath = "Data/C#Projects.csv";
            JavaScriptProjectsListingsPath = "Data/JavaScriptProjects.csv";
        }
        public static string CSharpProjectsListingsPath {get; private set;}
        public static string JavaScriptProjectsListingsPath { get; private set; }
    }
}
