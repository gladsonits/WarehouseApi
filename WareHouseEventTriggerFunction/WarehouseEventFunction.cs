using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using System;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Microsoft.ApplicationInsights;
using Azure.Messaging.ServiceBus;
using System.Text;

namespace WareHouseEventTriggerFunction
{
    public class WarehouseEventFunction
    {
        private readonly TelemetryClient telemetryClient;

        public WarehouseEventFunction(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        [FunctionName("WarehouseEventFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [CosmosDB(
            databaseName: "%CosmosDbDatabaseName%",
            containerName: "%CosmosDbCollectionName%",
            Connection = "CosmosDbConnectionStringSetting")] IAsyncCollector<InputModel> cosmosDbCollector,
         [ServiceBus(
            queueOrTopicName: "%ServiceBusTopicName%",
            Connection = "ServiceBusConnectionName")
            ] IAsyncCollector<ServiceBusMessage> serviceBusCollector,
        ILogger log, ITaskHelper taskHelper)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {

                // Step 1: Validate HTTP method (only support POST)
                if (req.Method.ToUpperInvariant() != "POST")
                {
                    log.LogError("Invalid HTTP method. Only POST requests are supported.");
                    return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
                }


                // Step 2: Deserialize HTTP request payload using the taskHelper
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var inputModel = taskHelper.Deserialize<InputModel>(requestBody);

                // Step 3: Save event data in CosmosDB
                await cosmosDbCollector.AddAsync(inputModel);

                // Step 4: Track CustomEvent in Azure Application Insights
                TrackCustomEvent(inputModel, req.Headers["CorrelationId"]);

                await PublishToServiceBus(requestBody, req.Headers["CorrelationId"], serviceBusCollector);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private void TrackCustomEvent(InputModel inputModel, string correlationId)
        {
            // Step 4: Track CustomEvent in Azure Application Insights
            var eventName = inputModel.EventName;

            // Create an EventTelemetry instance
            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry(eventName);

            // Set the CorrelationId property
            eventTelemetry.Properties["CorrelationId"] = correlationId;

            // Track the event
            telemetryClient.TrackEvent(eventTelemetry);

            // Flush telemetry to ensure it's sent immediately
            telemetryClient.Flush();
        }

        private async Task PublishToServiceBus(string requestBody, string correlationId, IAsyncCollector<ServiceBusMessage> serviceBusCollector)
        {
            //Publish the received payload to Azure Service Bus
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(requestBody))
            {
                CorrelationId = correlationId
            };
            await serviceBusCollector.AddAsync(message);
        }
    }
}
