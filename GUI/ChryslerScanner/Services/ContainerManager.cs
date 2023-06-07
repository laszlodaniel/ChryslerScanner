using System;
using SimpleInjector;

namespace ChryslerScanner.Services
{
    public static class ContainerManager
    {
        private static readonly Lazy<Container> _container = new Lazy<Container>(ConfigureServices);

        private static Container ConfigureServices() // for Dependency Injection
        {
            var container = new Container();

            container.Register<SerialService>(Lifestyle.Singleton);
            container.Register<MainForm>(Lifestyle.Singleton);
            container.Verify();

            return container;
        }

        public static Container Instance => _container.Value;
    }
}
