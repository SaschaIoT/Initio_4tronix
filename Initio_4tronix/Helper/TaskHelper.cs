using System;
using System.Threading;
using System.Threading.Tasks;

namespace Initio_4tronix.Helper
{
    public static class TaskHelper
    {
        public static async Task CancelTaskAfterTimeout(Func<CancellationToken, Task> operation, TimeSpan timeout)
        {
            var source = new CancellationTokenSource();
            var task = operation(source.Token);
            //After task starts timeout begin to tick
            source.CancelAfter(timeout);
            await task;
        }
    }
}
