using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class ReadLinesModule<TResult> : Module<TResult> where TResult : ReadLinesResult
    {
        public ReadLinesModule(IStageBuilder builder) 
            : base(builder) { }

        public string FilePath { get; init; }
        public Encoding Encoding { get; init; } = Encoding.Default;

        protected override async Task<TResult> DoWork(CancellationToken token)
        {
            var result = Activator.CreateInstance<TResult>();
            result.Lines = (await File.ReadAllLinesAsync(FilePath, Encoding, token)).ToArray();
            return result;
        }
    }
}
