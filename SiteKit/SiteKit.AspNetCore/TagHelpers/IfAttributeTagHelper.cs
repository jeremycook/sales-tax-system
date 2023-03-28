using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SiteKit.AspNetCore.TagHelpers
{
    /// <summary>
    /// Render the element if the condition is true.
    /// </summary>
    [HtmlTargetElement(Attributes = "sk-if")]
    public class IfAttributeTagHelper : TagHelper
    {
        [HtmlAttributeName("sk-if")]
        public bool Condition { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Condition)
            {
                output.SuppressOutput();
            }
        }
    }
}
