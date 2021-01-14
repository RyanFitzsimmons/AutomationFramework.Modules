using System;
using System.IO;
using System.Text;

namespace AutomationFramework.Modules
{
    public class FileListModule<TResult> : Module<TResult> 
        where TResult : FilePathResult
    {
        public FileListModule(IStageBuilder builder) 
            : base(builder) { }

        public override string Name { get; init; } = "File List";
        public FileInfo[] Files { get; init; }
        public bool IncludeDirectoryPath { get; init; }
        public string FileName { get; init; }
        public DirectoryInfo DestinationDirectory { get; init; }

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            var filePath = Path.Combine(DestinationDirectory.FullName, FileName);
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Default))
            {
                foreach (var file in Files)
                {
                    CheckForCancellation();
                    if (IncludeDirectoryPath) sw.WriteLine(file.FullName);
                    else sw.WriteLine(file.Name);
                }
            }

            result.FilePath = filePath;
            return result;
        }
    }
}
