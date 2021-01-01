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
    public class TransferFilesModule<TResult> : Module<TResult> 
        where TResult : TransferFilesModuleResult
    {
        public TransferFilesModule(IDataLayer dataLayer, IRunInfo runInfo, StagePath stagePath) : base(dataLayer, runInfo, stagePath)
        {
        }

        public override string Name { get; init; } = "Copy Files";

        public TransferTypes TransferType { get; set; }
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
                    var destinationDirectory = System.IO.Path.Combine(DestinationDirectory.FullName, file.Directory.FullName.Remove(0, SourceDirectory.FullName.Length));
                    if (!Directory.Exists(destinationDirectory))
                        GetRetryPolicy().Execute(() => Directory.CreateDirectory(destinationDirectory));
                    var fileDestination = System.IO.Path.Combine(destinationDirectory, file.Name);

                    GetRetryPolicy().Execute(() =>
                    {
                        Log(LogLevels.Information, $"Copying file \"{file.FullName}\" to \"{fileDestination}\"");
                        file.CopyTo(fileDestination, Overwrite);
                        destinationPaths.Add(fileDestination);
                    });

                    if (TransferType == TransferTypes.Move)
                        GetRetryPolicy().Execute(() =>
                        {
                            Log(LogLevels.Information, $"Deleting file \"{file.FullName}\"");
                            file.Delete();
                        });
                }
                catch 
                {
                    /// Logging the name of the file which failed to copy. The exception details
                    /// will be logged by the kernel
                    Log(LogLevels.Error, $"Failed to copy file \"{file.FullName}\"");
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
