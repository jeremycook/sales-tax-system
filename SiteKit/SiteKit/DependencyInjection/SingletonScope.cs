using Microsoft.Extensions.DependencyInjection;
using System;

namespace SiteKit.DependencyInjection
{
    public class SingletonScope : IServiceScope, IDisposable
    {
        private bool disposedValue;
        private readonly IServiceScope _scope;

        public SingletonScope(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
        }

        public IServiceProvider ServiceProvider => _scope.ServiceProvider;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
