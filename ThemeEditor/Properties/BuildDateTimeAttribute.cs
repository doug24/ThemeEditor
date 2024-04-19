using System;

namespace ThemeEditor.Properties
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateTimeAttribute(string date) : Attribute
    {
        public string Date { get; set; } = date;
    }
}
