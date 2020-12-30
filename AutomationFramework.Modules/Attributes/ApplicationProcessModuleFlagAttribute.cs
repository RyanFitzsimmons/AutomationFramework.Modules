using System;

namespace AutomationFramework.Modules.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ApplicationProcessModuleFlagAttribute : Attribute
    {
        public string Flag { get; set; }
    }
}
