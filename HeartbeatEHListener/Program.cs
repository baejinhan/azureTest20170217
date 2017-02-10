using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using HeartbeatEHListener;

namespace HeartbeatEHListener
{
    class Program
    {
        static void Main(string[] args)
        {
            //Event Hub Listener Host starting
            StartHost().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            //Event Hub Listener Unregistering
            host.UnregisterEventProcessorAsync().Wait();
        }

        static EventProcessorHost host;
        static HeartbeatEventProcessorFactory factory;

        private static async Task StartHost()
        {
            //Event Hub 
            var eventHubConnectionString = GetEventHubConnectionString();
            var storageConnectionString = GetStorageConnectionString();

            string hostname = Guid.NewGuid().ToString();
            host = new EventProcessorHost(
                hostname,
                Properties.Settings.Default.EventHubName,
                EventHubConsumerGroup.DefaultGroupName,
                eventHubConnectionString,
                storageConnectionString, Properties.Settings.Default.EventHubName.ToLowerInvariant());

            factory = new HeartbeatEventProcessorFactory(hostname);

            try
            {
                Console.WriteLine("{0} > Registering host: {1}", DateTime.Now.ToString(), hostname);
                var options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await host.RegisterEventProcessorFactoryAsync(factory);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
                Console.ResetColor();
            }
        }

        private static void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine("Received exception, action: {0}, messae： {1}.", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception.Message);
        }

        private static string GetStorageConnectionString()
        {
            return Properties.Settings.Default.StorageConnectionString;
        }

        private static string GetEventHubConnectionString()
        {
            return Properties.Settings.Default.EventHubConnectionString;
        }

    }
}
