using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class FileListModule<TResult> : Module<TResult> 
        where TResult : FilePathResult
    {
        public FileListModule(IStageBuilder builder) 
            : base(builder) { }

        public override string Name { get; init; } = "File List";
        public string[] FilePaths { get; init; }
        public bool IncludeDirectoryPath { get; init; }
        public string FilePath { get; init; }
        public bool Overwrite { get; init; }
        public Encoding Encoding { get; init; } = Encoding.Default;

        protected override async Task<TResult> DoWork(CancellationToken token)
        {
            var result = Activator.CreateInstance<TResult>();
            using (StreamWriter sw = new StreamWriter(FilePath, !Overwrite, Encoding))
            {
                foreach (var file in FilePaths)
                {
                    if (IncludeDirectoryPath) await sw.WriteLineAsync(new StringBuilder(file), token);
                    else await sw.WriteLineAsync(new StringBuilder(Path.GetFileName(file)), token);
                }
            }

            result.FilePath = FilePath;
            return result;
        }
    }
}
