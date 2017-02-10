using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;

namespace Receiver
{
    class SimpleEventProcessor : IEventProcessor
    {
        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);

            return Task.FromResult<object>(null);

        }
        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("SimpleEventProcessor initialized Partition: '{0}', offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            return Task.FromResult<object>(null);
        }
        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventdata in messages)
            {
                string data = Encoding.UTF8.GetString(eventdata.GetBytes());

                Console.WriteLine(string.Format("Message received. Partition: {0}, Data: {1} {2}", context.Lease.PartitionId, data, eventdata.EnqueuedTimeUtc));
            }


        }
}

}
