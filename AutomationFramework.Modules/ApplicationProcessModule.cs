﻿using AutomationFramework;
using AutomationFramework.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public abstract class ApplicationProcessModule<TResult> : Module<TResult> 
        where TResult : ApplicationProcessResult
    {
        protected ApplicationProcessModule(IStageBuilder builder) 
            : base(builder) { }

        public virtual string ApplicationPath { get; init; }
        public virtual string Arguments { get; init; }

        private ProcessStartInfo GetProcessStartInfo() =>
            new ProcessStartInfo
            {
                FileName = ApplicationPath,
                Arguments = Arguments ?? ArgumentsToString(GetArgumentsFromProperties()),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

        protected override async Task<TResult> DoWork(CancellationToken token)
        {
            var result = Activator.CreateInstance<TResult>();
            if (string.IsNullOrWhiteSpace(ApplicationPath)) return result;
            ProcessStartInfo psi = GetProcessStartInfo();
            Log(LogLevels.Information, psi.FileName + " " + psi.Arguments);

            try
            {
                using var process = Process.Start(psi);
                Log(LogLevels.Information, $"Process ID: {process.Id}");
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync(token);
                process.WaitForExit();
                process.OutputDataReceived -= Process_OutputDataReceived;
                process.ErrorDataReceived -= Process_ErrorDataReceived;
                result.ExitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                result.ExceptionMessage = ex.Message;
                result.Exception = ex.ToString();
                throw;
            }

            return result;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Log(LogLevels.Warning, e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Log(LogLevels.Information, e.Data);
        }

        private List<Tuple<string, string, bool>> GetArgumentsFromProperties()
        {
            var properties = GetArgumentProperties();
            var args = new List<Tuple<int?, string, string, bool>>();

            foreach (var property in properties)
            {
                var flagAtt = Attribute.GetCustomAttributes(property, typeof(ApplicationProcessModuleFlagAttribute)).SingleOrDefault() as ApplicationProcessModuleFlagAttribute;
                var orderAtt = Attribute.GetCustomAttributes(property, typeof(ApplicationProcessModuleOrderAttribute)).SingleOrDefault() as ApplicationProcessModuleOrderAttribute;
                var includeAtt = Attribute.GetCustomAttributes(property, typeof(ApplicationProcessModuleIncludeAttribute)).SingleOrDefault() as ApplicationProcessModuleIncludeAttribute;

                args.Add(new Tuple<int?, string, string, bool>(orderAtt?.OrderBy, flagAtt?.Flag, property.GetValue(this)?.ToString(), includeAtt.ForceQuotes));
            }

            List<Tuple<string, string, bool>> orderedArgs = new List<Tuple<string, string, bool>>();
            orderedArgs.AddRange(args.Where(x => x.Item1 != null).OrderBy(x => x.Item1).Select(x => new Tuple<string, string, bool>(x.Item2, x.Item3, x.Item4)));
            orderedArgs.AddRange(args.Where(x => x.Item1 == null).Select(x => new Tuple<string, string, bool>(x.Item2, x.Item3, x.Item4)));
            return orderedArgs;
        }

        private PropertyInfo[] GetArgumentProperties() =>
            this.GetType().GetProperties(
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static)
            .Where(x => Attribute.GetCustomAttributes(x, typeof(ApplicationProcessModuleIncludeAttribute)).Any())
            .ToArray();

        private static string ArgumentsToString(List<Tuple<string, string, bool>> arguments)
        {
            string s = "";
            foreach (var arg in arguments)
            {
                var flag = arg.Item1;
                var value = arg.Item2;
                var forceQuotes = arg.Item3;

                if (flag != null && flag.Length > 0)
                    s += flag + " ";

                if (string.IsNullOrWhiteSpace(value)) s += "\"\" ";
                else if (value.Contains(" ") || forceQuotes) s += "\"" + value + "\" ";
                else s += value + " ";
            }

            return s.Trim();
        }
    }
}
