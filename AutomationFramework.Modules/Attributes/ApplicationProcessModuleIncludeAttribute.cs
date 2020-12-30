using System;

namespace AutomationFramework.Modules.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ApplicationProcessModuleIncludeAttribute : Attribute
    {
        public bool ForceQuotes { get; set; }
    }
}
