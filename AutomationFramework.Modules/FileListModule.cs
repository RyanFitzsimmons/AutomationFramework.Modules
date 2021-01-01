using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class FileListModule<TResult> : Module<TResult> 
        where TResult : FileListModuleResult
    {
        public FileListModule(IDataLayer dataLayer, IRunInfo runInfo, StagePath stagePath) : base(dataLayer, runInfo, stagePath)
        {
        }

        public override string Name { get; init; } = "File List";

        public FileInfo[] Files { get; set; }

        public bool IncludeDirectoryPath { get; set; }

        public string FileName { get; set; }

        public DirectoryInfo DestinationDirectory { get; set; }

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            var filePath = System.IO.Path.Combine(DestinationDirectory.FullName, FileName);
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Default))
            {
                foreach (var file in Files)
                {
                    if (IncludeDirectoryPath) sw.WriteLine(file.FullName);
                    else sw.WriteLine(file.Name);
                }
            }

            result.FilePath = filePath;
            return result;
        }
    }
}
