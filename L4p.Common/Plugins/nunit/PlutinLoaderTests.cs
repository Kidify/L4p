using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace L4p.Common.Plugins.nunit
{
    [TestFixture]
    class PlutinLoaderTests
    {
        public class SomeClass : IServiceProvider
        {
            public object GetService(Type serviceType) { return null; }
        }

        [Test, Explicit]
        public void IntegrationTest()
        {
            var config = new PluginLoader.Config();
            var likeThis = typeof(SomeClass);

            var assemblyPath = likeThis.Assembly.Location;
            var assemblyName = Path.GetFileName(assemblyPath);
            var baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pluginFolder = Path.Combine(baseFolder, config.PluginFolder);
            var pluginPath = Path.Combine(pluginFolder, assemblyName);

            Directory.CreateDirectory(pluginFolder);
            File.Copy(assemblyPath, pluginPath, true);

            var instanceLoader = PluginLoader.New();

            var instance = instanceLoader.LoadImplementationOf<IServiceProvider>(likeThis);

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.GetType().FullName, Is.EqualTo(likeThis.FullName));

            Thread.Sleep(200);

            var instance2 = instanceLoader.LoadImplementationOf<IServiceProvider>(likeThis);
            Assert.That(instance2, Is.SameAs(instance));

            Thread.Sleep(2500);
            File.SetLastWriteTimeUtc(pluginPath, DateTime.UtcNow);

            var instance3 = instanceLoader.LoadImplementationOf<IServiceProvider>(likeThis);

            Assert.That(instance3, Is.Not.Null);
            Assert.That(instance3, Is.Not.SameAs(instance));

            var baseLocation = typeof (SomeClass).Assembly.Location;
            var location = instance.GetType().Assembly.Location;
            var location3 = instance3.GetType().Assembly.Location;

            Assert.That(location, Is.Not.EqualTo(baseLocation));
            Assert.That(location, Is.Not.EqualTo(location3));
        }
    }
}