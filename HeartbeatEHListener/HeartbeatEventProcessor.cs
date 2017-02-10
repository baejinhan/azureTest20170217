namespace HeartbeatEHListener
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.IO;
    using Newtonsoft.Json.Linq;

    class HeartbeatEventProcessor : IEventProcessor
    {
        private int totalMessages = 0;

        private Stopwatch checkpointStopWatch;

        //시작 시점 부터 메세지 수집 시작
        public HeartbeatEventProcessor()
        {
            this.LastMessageOffset = "-1";
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get
            {
                return this.totalMessages;
            }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            // 이벤트 프로세서 초기화.
            Console.WriteLine("{0} > Processor Initializing for PartitionId '{1}' and Owner: {2}.", DateTime.Now.ToString(), context.Lease.PartitionId, context.Lease.Owner ?? string.Empty);
            this.Context = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            this.IsInitialized = true;


            return Task.FromResult<object>(null);
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
            {
                foreach (EventData message in messages)
                {
                    string heartbeatmessage = Encoding.UTF8.GetString(message.GetBytes());
                    Task.Factory.StartNew(() => UpdateTableAsync(context, heartbeatmessage));
                    string consoleMsg = heartbeatmessage;//string.Format("{0} > received message: {1} at partition {2}, owner: {3}, offset: {4}", DateTime.Now.ToString(), heartbeatmessage, context.Lease.PartitionId, context.Lease.Owner, message.Offset);
                    //Console.WriteLine(consoleMsg);
                    // increase the totally amount.
                    Interlocked.Increment(ref this.totalMessages);
                    this.LastMessageOffset = message.Offset;
                }

                if (this.IsClosed)
                {
                    this.IsReceivedMessageAfterClose = true;
                }

                if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(1))
                {
                    //await context.CheckpointAsync();
                    //this.checkpointStopWatch.Restart();

                    lock (this)
                    {
                        this.checkpointStopWatch.Restart();
                        return context.CheckpointAsync();
                        //this.checkpointStopWatch.Reset();
                        //return context.CheckpointAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return Task.FromResult<object>(null);
        }

        private Task UpdateTableAsync(PartitionContext context, string messagingtext)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable heartbeatTable = tableClient.GetTableReference("heartbeat");
            if (heartbeatTable == null)
            {
                heartbeatTable.CreateAsync().Wait();
            }

            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(messagingtext));
            StreamReader reader = new StreamReader(memStream);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {

                var json = JsonConvert.DeserializeObject(line);

                JObject jParse = JObject.Parse(Convert.ToString(json));
                string DeviceID = (string)jParse["device_id"];
                string lastdatarecieved = (string)jParse["lastdatarecieved"];

                HeartbeatEntity heartbeatObj = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<HeartbeatEntity>(line)).Result;
                TableOperation insertorreplace = TableOperation.InsertOrReplace(heartbeatObj);
                Task.Factory.StartNew(() => heartbeatTable.ExecuteAsync(insertorreplace));

                Console.WriteLine(DeviceID + ", " + lastdatarecieved + ", " + context.Lease.PartitionId);

            }

            return Task.FromResult(true);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            // where you close the processor.
            Console.WriteLine("{0} > Close called for processor with PartitionId '{1}' and Owner: {2} with reason '{3}'.", DateTime.Now.ToString(), context.Lease.PartitionId, context.Lease.Owner ?? string.Empty, reason);
            this.IsClosed = true;
            this.checkpointStopWatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();
            return context.CheckpointAsync();
        }

        protected virtual void OnProcessorClosed()
        {
            EventHandler handler = this.ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
