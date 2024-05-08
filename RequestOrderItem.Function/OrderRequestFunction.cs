using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Web.Http.Results;

public  class OrderRequestFunction
{
    private readonly ILogger<OrderRequestFunction> _logger;
    private readonly string connectionString = Environment.GetEnvironmentVariable("ConnectionString");

    public OrderRequestFunction(ILogger<OrderRequestFunction> logger)
    {
        _logger = logger;
    }

    [Function("OrderRequest")]
    public async Task<bool> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        _logger.LogInformation($"New Order Request ==> {requestBody}");

        if (requestBody != null)
        {
            // Store the message in Azure Queue Storage
            await EnqueueMessageAsync(requestBody);

            return true;
        }
        else
        {
            _logger.LogError($"Invalid request body ==> {req.Body}");
            return false;
        }
    }

    private async Task EnqueueMessageAsync(string message)
    {
        // Retrieve the storage account connection string from environment variable
        string storageConnectionString = connectionString;

        // Retrieve storage account
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        // Create the queue client
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        // Retrieve a reference to a queue
        CloudQueue queue = queueClient.GetQueueReference("process-order-queue");

        // Create the queue if it doesn't already exist
        await queue.CreateIfNotExistsAsync();

        // Create a message and add it to the queue
        CloudQueueMessage queueMessage = new CloudQueueMessage(message);
        await queue.AddMessageAsync(queueMessage);
    }
}
