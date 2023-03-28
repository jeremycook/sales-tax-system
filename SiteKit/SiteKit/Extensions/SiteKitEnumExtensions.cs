using Humanizer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SiteKit.Extensions
{
    public static class SiteKitEnumExtensions
    {
        public static T? GetAttributeOfType<T>(this Enum @enum) where T : System.Attribute
        {
            var type = @enum.GetType();
            var memberInfo = type.GetMember(@enum.ToString())[0];
            return memberInfo.GetCustomAttribute<T>(inherit: false);
        }

        public static string GetDisplayName(this Enum @enum)
        {
            return @enum.GetAttributeOfType<DisplayAttribute>()?.Name ?? @enum.Humanize();
        }
    }
}
