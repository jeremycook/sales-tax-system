using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SiteKit.AspNetCore.TagHelpers
{
    [HtmlTargetElement("sk-if")]
    public class IfTagHelper : TagHelper
    {
        [HtmlAttributeName("condition")]
        public bool Condition { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("condition");

            if (!Condition)
            {
                output.SuppressOutput();
            }
        }
    }
}
