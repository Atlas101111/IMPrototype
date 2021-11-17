using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
using Unity.Interception;
using Unity.Lifetime;
using Unity.Microsoft.DependencyInjection;
using ImPrototype.Hubs;

namespace ImPrototype
{
    public class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> _container =
            new Lazy<IUnityContainer>(() =>
            {
                var container = new UnityContainer();
                RegisterComponents(container);
                return container;
            });

        public static IUnityContainer GetConfiguredContainer()
        {
            return _container.Value;
        }

        private static void RegisterComponents(UnityContainer container)
        {
            container.AddNewExtension<Interception>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            serviceCollection.BuildServiceProvider(container);

            container.RegisterType<ConnectionManager, ConnectionManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor()
                );

            container.RegisterType<ImMongoClient, ImMongoClient>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&directConnection=true&ssl=false")
                );

            container.RegisterType<OfflineMongoAccessor, OfflineMongoAccessor>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ImMongoClient>()
                    )
                );
        }
    }
}
