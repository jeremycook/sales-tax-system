using System;

namespace SiteKit.DependencyInjection
{
    /// <summary>
    /// Mark an implementation as a scoped service and optionally provide the <see cref="ServiceType"/>
    /// if different from the implementation.
    /// </summary>
    public class ScopedAttribute : Attribute
    {
        public ScopedAttribute()
        {
        }
        public ScopedAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type? ServiceType { get; set; }
    }
}
