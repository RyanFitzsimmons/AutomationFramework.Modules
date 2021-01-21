using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public static class IOAsyncExtensions
    {
        public static async Task CopyToAsync(this FileInfo file, string destination, bool overwrite, CancellationToken token)
        {
            if (!overwrite && File.Exists(destination))
                throw new IOException($"The file \"{destination}\" already exists.");

            using FileStream sourceStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream destinationStream = File.Create(destination);
            await sourceStream.CopyToAsync(destinationStream, token);
        }
    }
}
