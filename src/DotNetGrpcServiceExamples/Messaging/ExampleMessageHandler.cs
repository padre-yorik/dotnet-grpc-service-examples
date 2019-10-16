using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models;
using NewVoiceMedia.Messaging.Client;
using NewVoiceMedia.Messaging.Models;
using NewVoiceMedia.OtherExample.Messages;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Messaging
{
public class ExampleMessageHandler : TypedHandler<OtherExampleFooCreatedV1>
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ExampleMessageHandler> _logger;

    public ExampleMessageHandler(IMessagePublisher messagePublisher, ILogger<ExampleMessageHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }
    
    /// <summary>
    /// An example message handler, overkill in practice
    ///  - Logs with very verbose detail
    ///  - Optionally extends visibility timeout
    ///  - Publishes dotnetgrpcserviceexamples-bar-created-v1
    /// This would be your actual method for handling your message when being dequeued.
    /// </summary>
    public override async Task<bool> HandleMessage(
        OtherExampleFooCreatedV1 message,
        MessageMetadata metadata,
        CancellationToken cancellationToken,
        Func<TimeSpan, Task<bool>> extendVisibility)
    {
        var sb = new StringBuilder();
        sb.Append("New message received. ");
        sb.Append($"Id: {metadata.Id}. ");
        sb.Append($"MessageType: {metadata.MessageType}. ");
        sb.Append($"AccountId: {metadata.AccountId}. ");
        sb.Append($"CorrelationId: {metadata.CorrelationId}. ");
        sb.Append($"CreatedTimeStamp: {metadata.CreatedTimeStamp}. ");
        sb.Append($"InteractionGuid: {metadata.InteractionGuid}. ");
        sb.Append($"MetadataVersion: {metadata.MetadataVersion}. ");
        sb.AppendLine();

        sb.Append($"DebugData.ProducerName: {metadata.DebugData.ProducerName}. ");
        sb.Append($"DebugData.ProducerVersion: {metadata.DebugData.ProducerVersion}. ");
        sb.Append($"DebugData.MessageSchemaVersion: {metadata.DebugData.MessageSchemaVersion}. ");
        sb.Append($"DebugData.AdditionalInfo: {metadata.DebugData.AdditionalInfo}. ");
        sb.AppendLine();
        
        _logger.LogInformation(sb.ToString());
        
        // You would extend visibility timeout of a message if you want to keep a handle on it because
        // you are still processing it and do not want another instance of your service to queue
        // the message and start processing it too.
        if (message.SetVisiblityTimeout != null)
        {
            var result = await extendVisibility(message.SetVisiblityTimeout.Value);
            _logger.LogInformation($"Setting visibility timeout of {metadata.Id} to {message.SetVisiblityTimeout.Value}. Result from AWS: {result}");

            // This is how you would heartbeat/keep a handle on the current receipt of a message, by keeping
            // on extending the visibilityTimeOut
            if (message.SimulateHeartBeat == true)
            {
                // This is intended to be akin to a "I'm still processing" loop
                await Task.Delay(5000); // An arbitrary sleep aka processing time.
                result = await extendVisibility(message.SetVisiblityTimeout.Value);
                _logger.LogInformation($"Setting visibility timeout for a second time of {metadata.Id} to {message.SetVisiblityTimeout.Value}. Result from AWS: {result}");
            }
        }

        // Publish a dotnetgrpcserviceexamples-bar-created-v1 message, as an example of how consuming a message may result in producing another
        var newMessage = new Message(
            new DotNetGrpcServiceExamplesBarCreatedV1
            {
                Bar = message.Foo
            },
            new MessageMetadata(metadata.CorrelationId, new DebugData(Program.ServiceName, BuildInfo.Version))
            {
                AccountId = metadata.AccountId
            });

        await _messagePublisher.PublishAsync(newMessage);
        
        return message.IsToTreatAsSuccessfulOutcome ?? true;
    }
}
}
