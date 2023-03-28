using Microsoft.Extensions.DependencyInjection;

#nullable disable

namespace SiteKit.DependencyInjection
{
    /// <summary>
    /// A convenient, strong typed way of returning the <typeparamref name="TService"/> from the <see cref="SingletonScope"/>.
    /// Remember to register instances of the <see cref="Singleton{TService}"/>.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public class Singleton<TService>
    {
        public Singleton(SingletonScope singletonScope)
        {
            Service = singletonScope.ServiceProvider.GetRequiredService<TService>();
        }

        public TService Service { get; }
    }
}
