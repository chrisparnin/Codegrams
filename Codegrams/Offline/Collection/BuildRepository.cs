using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codegrams.Collection
{
    public class BuildRepository
    {
        public static void DownloadProjects(
                IRepositorySource source, 
                string outputDir, 
                IEnumerable<string> projects
            )
        {
            if (!File.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach( var project in projects )
            {
                string projectName =
                    project.Split('/').Reverse().Skip(1).First() + "_" +
                    project.Split('/').Last();

                var output = Path.Combine(outputDir, projectName + ".zip");

                if (source.DownloadZip(project, output))
                {
                    Console.WriteLine("Downloaded {0}", project);
                }
                Thread.Sleep(10000);
            }
        }
    }

    public static class Extensions
    {
        private static Random random = new Random();

        public static T GetRandomElement<T>(this IEnumerable<T> list)
        {
            // If there are no elements in the collection, return the default value of T
            if (list.Count() == 0)
                return default(T);

            return list.ElementAt(random.Next(list.Count()));
        }
    }

}
