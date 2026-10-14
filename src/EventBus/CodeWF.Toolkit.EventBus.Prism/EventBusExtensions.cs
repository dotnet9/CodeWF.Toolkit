using LibraryResources = global::CodeWF.Toolkit.EventBus.Prism.I18n.ResourceKeys;
using CodeWF.Toolkit.EventBus;
using DryIoc;
using Prism.Ioc;
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.Toolkit.EventBus.Prism
{
    /// <summary>
    /// DryIoc / Prism 场景下的事件总线扩展。
    /// </summary>
    public static class EventBusExtensions
    {
        private static Assembly[] GetAssemblies(Assembly[] assemblies)
        {
            var requestedAssemblies = assemblies ?? Array.Empty<Assembly>();
            if (requestedAssemblies.Any(assembly => assembly == null))
            {
                throw new ArgumentException(global::CodeWF.Toolkit.EventBus.Prism.LibraryText.Get(LibraryResources.Registration.NullAssembly), nameof(assemblies));
            }

            var entryAssembly = Assembly.GetEntryAssembly();
            var assembliesToScan = requestedAssemblies.Length > 0
                ? requestedAssemblies.Concat(entryAssembly == null ? Array.Empty<Assembly>() : new[] { entryAssembly })
                : entryAssembly == null
                    ? AppDomain.CurrentDomain.GetAssemblies()
                    : new[] { entryAssembly };

            return assembliesToScan
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// 注册事件总线及自动发现的实例处理器。
        /// </summary>
        public static IContainerRegistry AddEventBus(this IContainerRegistry services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.RegisterSingleton<IEventBus, CodeWF.Toolkit.EventBus.EventBus>();

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.Toolkit.EventBus.EventBusExtensions.HandleEventObject(type => services.RegisterScoped(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);

            return services;
        }

        /// <summary>
        /// 启动事件总线，并通过 DryIoc 作用域解析实例处理器。
        /// </summary>
        public static void UseEventBus(this IContainer app, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(app);
            if (app.Resolve<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException(global::CodeWF.Toolkit.EventBus.Prism.LibraryText.Get(LibraryResources.Registration.AddBeforeUse));
            }

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.Toolkit.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
            messenger.RegisterServiceHandlerAction((type, action) =>
            {
                using var scope = app.OpenScope();
                var obj = scope.Resolve(type);
                action(obj);
            });
            messenger.Subscribe(allAssemblies);
        }
    }
}
