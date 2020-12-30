using System;

namespace AutomationFramework.Modules.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ApplicationProcessModuleOrderAttribute : Attribute
    {
        public int OrderBy { get; set; }
    }
}
