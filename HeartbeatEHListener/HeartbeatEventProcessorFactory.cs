using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace HeartbeatEHListener
{
    class HeartbeatEventProcessorFactory : IEventProcessorFactory
    {
        private readonly ConcurrentDictionary<string, HeartbeatEventProcessor> eventProcessors = new ConcurrentDictionary<string, HeartbeatEventProcessor>();

        private readonly ConcurrentQueue<HeartbeatEventProcessor> closedProcessors = new ConcurrentQueue<HeartbeatEventProcessor>();

        public HeartbeatEventProcessorFactory(string hostname)
        {
            this.HostName = hostname;
        }

        public string HostName { get; private set; }

        public int ActiveProcesors
        {
            get
            {
                return this.eventProcessors.Count;
            }
        }

        public int TotalMessages
        {
            get
            {
                var amount = this.eventProcessors.Select(p => p.Value.TotalMessages).Sum();
                amount += this.closedProcessors.Select(p => p.TotalMessages).Sum();
                return amount;
            }
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new HeartbeatEventProcessor();
            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public Task WaitForAllProcessorsInitialized(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsInitialized, timeout);
        }

        public Task WaitForAllProcessorsClosed(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsClosed, timeout);
        }

        public async Task WaitForAllProcessorsCondition(Func<HeartbeatEventProcessor, bool> predicate, TimeSpan timeout)
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(2);
            while (!this.eventProcessors.Values.All(predicate))
            {
                if (timeout > TimeSpan.Zero)
                {
                    timeout = timeout.Subtract(sleepInterval);
                }
                else
                {
                    throw new TimeoutException("Condition not satisfied within expected timeout.");
                }

                await Task.Delay(sleepInterval);
            }
        }

        private void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as HeartbeatEventProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                this.closedProcessors.Enqueue(processor);
            }
        }
    }
}
