using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://testeventhubbaejinhan.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DfW3OKNRzyX0UmL+5p/z/91CwPk/9H+1v+F5BEM+en0=";
            string eventHubName = "eventhub";
            string storageAccountName = "teststoragebjh";
            string storageAccountKey = "y3jzaNgYNpoEl4yfOgZwBKNPBSkzua0LDYdUVO3SnP40E3V71X+yEGi7FEa3XCWqQQQcFp7C9pTkUZhG+u//Jw==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(options).Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }

}
