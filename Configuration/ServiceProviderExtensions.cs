using Microsoft.Extensions.DependencyInjection;
using System;

namespace RestfulBookerTests.Configuration
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Safely resolves a required service from the provider and throws a clear error if not found.
        /// </summary>
        public static T GetRequiredServiceSafe<T>(this IServiceProvider provider, string contextInfo = "")
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider), "ServiceProvider cannot be null.");

            var service = provider.GetService<T>();
            if (service == null)
            {
                var message = $"Failed to resolve service of type '{typeof(T).FullName}'.";
                if (!string.IsNullOrEmpty(contextInfo))
                {
                    message += $" Context: {contextInfo}.";
                }

                message += " Ensure it is registered in ServiceCollectionExtensions.AddRestfulBookerServices().";

                throw new InvalidOperationException(message);
            }

            return service;
        }
    }
}
