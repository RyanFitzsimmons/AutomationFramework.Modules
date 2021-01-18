using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class RegexFindFilesModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public RegexFindFilesModule(IStageBuilder builder) 
            : base(builder) { }

        public string RegexPattern { get; init; }
        public string SourceDirectoryPath { get; init; }
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

                if (string.IsNullOrWhiteSpace(RegexPattern))
                {
                    Log(LogLevels.Error, "Regex pattern is empty");
                    return false;
                }

                return true;
            }
        }

        protected override async Task<TResult> DoWork(CancellationToken token)
        {
            var result = Activator.CreateInstance<TResult>();
            if (!IsValid) throw new Exception("Invalid Module Setup");
            var allFiles = SourceDirectory.GetFiles("*",
                (Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            List<string> matches = new();
            foreach (var file in allFiles)
            {
                if (Regex.IsMatch(file.Name, RegexPattern))
                    matches.Add(file.FullName);
            }
            result.FilePaths = matches.ToArray();
            return await Task.FromResult(result);
        }
    }
}
