using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class FileListModule<TResult> : Module<TResult> 
        where TResult : FilePathResult
    {
        public FileListModule(IStageBuilder builder) : base(builder)
        {
        }

        public override string Name { get; init; } = "File List";
        public FileInfo[] Files { get; init; }
        public bool IncludeDirectoryPath { get; init; }
        public string FileName { get; init; }
        public DirectoryInfo DestinationDirectory { get; init; }

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
