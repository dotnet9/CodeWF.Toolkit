using CodeWF.Toolkit.Network.Tests.EventBus.Commands;
using CodeWF.Toolkit.Network.Tests.EventBus.Queries;

namespace CodeWF.Toolkit.Network.Tests.EventBus.Handlers
{
    internal class StaticHandler
    {
        public static int TestCount = 0;

        [EventHandler]
        public static void ReceiveAddCommand(TestAddCommand command)
        {
            TestCount++;
        }

        [EventHandler]
        public static void ReceiveSubtractCommand(TestSubtractCommand command)
        {
            TestCount--;
        }

        [EventHandler]
        public static void ReceiveStaticQuery(TestQuery query)
        {
            query.Result = TestCount;
        }
    }
}