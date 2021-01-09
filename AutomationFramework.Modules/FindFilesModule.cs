using System;
using System.IO;
using System.Linq;

namespace AutomationFramework.Modules
{
    public class FindFilesModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public FindFilesModule(IStageBuilder builder) : base(builder)
        {
        }

        public string SourceDirectoryPath { get; init; }
        public string SearchPattern { get; set; }
        public bool Recursive { get; init; }
        private DirectoryInfo SourceDirectory => new DirectoryInfo(SourceDirectoryPath);

        private bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceDirectoryPath))
                {
                    Log(LogLevels.Error, "Source directory path is empty");
                    return false;
                }

                return true;
            }
        }

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            if (!IsValid) throw new Exception("Invalid Module Setup");
            var files = SourceDirectory.GetFiles(
                (string.IsNullOrWhiteSpace(SearchPattern) ? "*" : SearchPattern), 
                (Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            result.FilePaths = files.Select(x => x.FullName).ToArray();
            return result;
        }
    }
}