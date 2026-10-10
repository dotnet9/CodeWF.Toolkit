using CodeWF.Toolkit.Network.Tests.EventBus.Commands;
using CodeWF.Toolkit.Network.Tests.EventBus.Queries;

namespace CodeWF.Toolkit.Network.Tests.EventBus.Handlers
{
    internal class StaticHandler2
    {
        [EventHandler]
        public static void ReceiveAddCommand(TestAddCommand command)
        {
            StaticHandler.TestCount++;
        }

        [EventHandler]
        public static void ReceiveSubtractCommand(TestSubtractCommand command)
        {
            StaticHandler.TestCount--;
        }

        [EventHandler]
        public static void ReceiveStaticQuery(TestQuery query)
        {
            query.Result = StaticHandler.TestCount;
        }
    }
}