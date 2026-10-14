using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.EventBus
{
    public partial class EventBus
    {
        /// <summary>
        /// 统一执行同步/异步处理器，调用方按返回值是否为空判断是否需要等待。
        /// </summary>
        private static Task InvokeHandler(Delegate handler, object command)
        {
            object? result;
            try
            {
                result = handler.DynamicInvoke(command);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }

            if (handler.Method.ReturnType == typeof(Task))
            {
                return (Task?)result ?? Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task InvokeAutoHandler(DiscoveredHandlerMethod handler, Type commandType, object command)
        {
            Task? task = null;
            var methodInfo = handler.Method;
            var serviceHandlerAction = _serviceHandlerAction ?? throw new InvalidOperationException(
                global::CodeWF.Toolkit.EventBus.LibraryText.Get("Toolkit.EventBus.Registration.ResolverRequired") +
                global::CodeWF.Toolkit.EventBus.LibraryText.Get("Toolkit.EventBus.Registration.ResolverInstructions"));
            serviceHandlerAction(handler.RecipientType, recipient =>
            {
                if (recipient == null)
                {
                    throw new InvalidOperationException(
                        global::CodeWF.Toolkit.EventBus.LibraryText.Format("Toolkit.EventBus.Registration.ResolverReturnedNull", handler.RecipientType.FullName));
                }

                var delegateType = methodInfo.ReturnType == typeof(Task)
                    ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                    : typeof(Action<>).MakeGenericType(commandType);
                var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                task = InvokeHandler(delegateInstance, command);
            });

            return task ?? Task.CompletedTask;
        }

        private void ThrowIfSynchronousPublishIsNotSupported(Type commandType)
        {
            var handlers = GetSubscriptionSnapshot(commandType);
            if (handlers.Any(handler => handler.Action.Method.ReturnType == typeof(Task)))
            {
                throw new InvalidOperationException(
                    global::CodeWF.Toolkit.EventBus.LibraryText.Get("Toolkit.EventBus.Dispatch.AsyncHandlerRequiresAsync"));
            }

            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Any(handler => handler.Method.ReturnType == typeof(Task)))
            {
                throw new InvalidOperationException(
                    global::CodeWF.Toolkit.EventBus.LibraryText.Get("Toolkit.EventBus.Dispatch.AsyncHandlerRequiresAsync"));
            }
        }

        /// <summary>
        /// 同步发布命令。
        /// </summary>
        public void Publish<TCommand>(TCommand command) where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            ThrowIfSynchronousPublishIsNotSupported(command.GetType());
            var publishing = PublishAsync(command);
            if (!publishing.IsCompleted)
            {
                // 校验通过后任务仍未完成，说明发布期间并发订阅了异步处理器；
                // 此时阻塞等待可能在持有同步上下文的线程（如 UI 线程）上死锁，
                // 因此与异步处理器保持一致的快速失败策略。
                throw new InvalidOperationException(
                    global::CodeWF.Toolkit.EventBus.LibraryText.Get("Toolkit.EventBus.Dispatch.AsyncHandlerRequiresAsync"));
            }

            publishing.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步执行查询。
        /// </summary>
        public T Query<T>(Query<T> query)
        {
            Publish(query);
            return query.Result;
        }

        /// <summary>
        /// 异步发布命令。
        /// 先执行手动注册的处理器，再执行程序集扫描得到的实例处理器。
        /// </summary>
        public async Task PublishAsync<TCommand>(TCommand command) where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();
            var handlers = GetSubscriptionSnapshot(commandType);
            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Length > 0)
            {
                ThrowIfAutoHandlersAreNotConfigured();
            }

            var invocations = new List<(int Order, Func<Task> Invoke)>(handlers.Length + autoHandlers.Length);
            foreach (var handler in handlers)
            {
                var currentHandler = handler;
                invocations.Add((currentHandler.Order,
                    () => InvokeHandler(currentHandler.Action, command)));
            }

            foreach (var handler in autoHandlers)
            {
                var currentHandler = handler;
                invocations.Add((currentHandler.Order,
                    () => InvokeAutoHandler(currentHandler, commandType, command)));
            }

            foreach (var invocation in invocations.OrderBy(item => item.Order))
            {
                await invocation.Invoke();
            }
        }

        /// <summary>
        /// 异步执行查询。
        /// </summary>
        public async Task<T> QueryAsync<T>(Query<T> query)
        {
            await PublishAsync(query);
            return query.Result;
        }
    }
}
