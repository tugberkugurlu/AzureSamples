using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusSample
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (namespaceManager.QueueExists("TestQueue") == false)
            {
                namespaceManager.CreateQueue(new QueueDescription("TestQueue")
                {
                    LockDuration = TimeSpan.FromMinutes(5)
                });
            }

            Task sendTask = StartSending(cts.Token);
            Task receiveTask = StartReciving(cts.Token);

            while (true)
            {
                ConsoleKeyInfo val = Console.ReadKey();
                if (val.Key == ConsoleKey.C)
                {
                    cts.Cancel();
                    break;
                }
            }

            try
            {
                sendTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            try
            {
                receiveTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        static async Task StartSending(CancellationToken token = default(CancellationToken))
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            QueueClient queueClient = QueueClient.CreateFromConnectionString(connectionString, "TestQueue");

            while (true)
            {
                token.ThrowIfCancellationRequested();

                var message = new BrokeredMessage(Guid.NewGuid().ToString("N"));
                message.Properties["TestProperty"] = Guid.NewGuid().ToString("N");
                await queueClient.SendAsync(message).ConfigureAwait(false);

                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        static async Task StartReciving(CancellationToken token = default(CancellationToken))
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            QueueClient queueClient = QueueClient.CreateFromConnectionString(connectionString, "TestQueue", ReceiveMode.PeekLock);

            while (true)
            {
                token.ThrowIfCancellationRequested();

                BrokeredMessage message = await queueClient.ReceiveAsync().ConfigureAwait(false);
                if (message != null)
                {
                    try
                    {
                        Console.WriteLine("Message TimeToLive: " + message.TimeToLive);
                        Console.WriteLine("Body: " + message.GetBody<string>());
                        Console.WriteLine("Delivery Count: " + message.DeliveryCount);
                        Console.WriteLine("MessageID: " + message.MessageId);
                        Console.WriteLine("Test Property: " + message.Properties["TestProperty"]);
                        Console.WriteLine("===========================================================");
                        Console.Write(Environment.NewLine);

                        // when message.LockedUntilUtc is not eneough, renew the lock:
                        // await message.RenewLockAsync().ConfigureAwait(false);

                        // Remove message from queue
                        await message.CompleteAsync().ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // Indicate a problem, unlock message in queue
                        message.Abandon();
                    }
                }
            }
        }
    }
}