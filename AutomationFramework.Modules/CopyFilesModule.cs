using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class CopyFilesModule<TDataLayer, TResult> : Module<TDataLayer, TResult> 
        where TDataLayer : IModuleDataLayer
        where TResult : CopyFilesModuleResult
    {
        public override string Name { get; set; } = "Copy Files";

        public string SearchPattern { get; set; }
        public DirectoryInfo SourceDirectory { get; set; }
        public SearchOption SearchOption { get; set; }
        public DirectoryInfo DestinationDirectory { get; set; }
        public bool Overwrite { get; set; }

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            var destinationPaths = new List<string>();
            var files = SourceDirectory.GetFiles(SearchPattern, SearchOption);
            foreach (var file in files)
            {
                try
                {
                    var destinationDirectory = Path.Combine(DestinationDirectory.FullName, file.Directory.FullName.Remove(0, SourceDirectory.FullName.Length));
                    if (!Directory.Exists(destinationDirectory))
                        GetRetryPolicy().Execute(() => Directory.CreateDirectory(destinationDirectory));
                    var fileDestination = Path.Combine(destinationDirectory, file.Name);

                    GetRetryPolicy().Execute(() =>
                    {
                        Logger.Information(StagePath, $"Copying file \"{file.FullName}\" to \"{fileDestination}\"");
                        file.CopyTo(fileDestination, Overwrite);
                        destinationPaths.Add(fileDestination);
                    });
                }
                catch (IOException ex)
                {
                    Logger.Fatal(StagePath, $"Failed to copy file \"{file.FullName}\"");
                    Logger.Fatal(StagePath, ex.ToString());
                    throw;
                }
            }

            result.FilePaths = destinationPaths.ToArray();
            return result;
        }

        private static RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<IOException>()
                .WaitAndRetry(25,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
