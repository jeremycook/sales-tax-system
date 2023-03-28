using System.ComponentModel.DataAnnotations;

namespace SiteKit.ComponentModel
{
    public class BooleanAttribute : DataTypeAttribute
    {
        public BooleanAttribute(string trueText, string falseText, string? nullText = null) : base("Boolean")
        {
            TrueText = trueText;
            FalseText = falseText;
            NullText = nullText;
        }

        public string TrueText { get; }
        public string FalseText { get; }
        public string? NullText { get; }
    }
}
