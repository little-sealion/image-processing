using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using ImageProcessing.API.Services.Interfaces;
using Azure.Messaging.ServiceBus;

namespace ImageProcessing.API.Services;

public class QueuesManagement : IQueuesManagement
{
    public async Task<bool> SendMessage<T>(T serviceMessage, string queue, string connectionString)
    {
        try
        {
            // Create a queue client
           await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(queue);

            var msgBody = JsonSerializer.Serialize(serviceMessage);

            // Create a Service Bus Message
            ServiceBusMessage msg = new ServiceBusMessage(msgBody);

            await sender.SendMessageAsync(msg);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
       
    }
}
