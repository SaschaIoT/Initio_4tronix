using System.Threading;

namespace Initio_4tronix.Helper
{
    public static class QueuedLock
    {
        private static object innerLock = new object();
        private static volatile int ticketsCount = 0;
        private static volatile int ticketToRide = 1;

        public static void Enter()
        {
            int myTicket = Interlocked.Increment(ref ticketsCount);
            Monitor.Enter(innerLock);
            while (true)
            {

                if (myTicket == ticketToRide)
                {
                    return;
                }
                else
                {
                    Monitor.Wait(innerLock);
                }
            }
        }

        public static void Exit()
        {
            Interlocked.Increment(ref ticketToRide);
            Monitor.PulseAll(innerLock);
            Monitor.Exit(innerLock);
        }
    }
}
