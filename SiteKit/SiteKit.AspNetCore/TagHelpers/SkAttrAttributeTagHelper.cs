using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace SiteKit.AspNetCore.TagHelpers
{
    /// <summary>
    /// Render an attribute if a condition is true.
    /// </summary>
    [HtmlTargetElement(Attributes = BoolPrefix + "*")]
    public class SkAttrAttributeTagHelper : TagHelper
    {
        private const string BoolPrefix = "sk-attr-";

        private IDictionary<string, bool>? _boolValues;

        [HtmlAttributeName(DictionaryAttributePrefix = BoolPrefix)]
        public IDictionary<string, bool> AttrConditions
        {
            get
            {
                if (_boolValues == null)
                {
                    _boolValues = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                }

                return _boolValues;
            }
            set
            {
                _boolValues = value;
            }
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_boolValues != null)
            {
                foreach (var item in _boolValues)
                {
                    if (item.Value)
                    {
                        output.Attributes.Add(item.Key, null);
                    }
                }
            }
        }
    }
}
