using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewVoiceMedia.AspNetCore.Mvc;
using NewVoiceMedia.Claims;
using NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models;
using NewVoiceMedia.Messaging.Client;
using NewVoiceMedia.Messaging.Models;
using NewVoiceMedia.OtherExample.Messages;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Controllers
{
    [Route("/messaging")]
    [ApiController]
    [ApiVersion("3")]
    public class MessagingV3Controller : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;

        public MessagingV3Controller(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        [Description("Simulate message publish from another service which will loopback to the example service queue.")]
        [HttpPost("publishmessageminimal")]
        public async Task<ActionResult> PublishMessageMinimal([FromBody] OtherExampleFooCreatedV1 message)
        {
            var account = User.GetAccountId();
            var debugData = new DebugData(Program.ServiceName, BuildInfo.Version);
            var metadata = new MessageMetadata(Request.Headers.GetCorrelationId(), debugData)
            {
                AccountId = (long) account
            };
            var newMessage = new Message(message, metadata);
            await _messagePublisher.PublishAsync(newMessage);

            return Ok();
        }

        [Description("Simulate message publish from another service which will loopback to the example service queue. With options to simulate load testing.")]
        [HttpPost("publish")]
        public async Task<ActionResult> PublishMessage([FromBody] SendMessageRequest request)
        {
            if (request.MessageContent.SimulateHeartBeat.HasValue && request.MessageContent.SetVisiblityTimeout < TimeSpan.FromSeconds(5))
            {
                return BadRequest("If you want to simulate a heartbeat, you need to provide a value greater than 5 for setvisibilitytimeout as the heartbeat simulation sleeps for 5s");
            }

            // The account ID should be present in the Identity Token
            var account = User.GetAccountId();
            var interactionGuid = request.InteractionGuid?.ToString() ?? string.Empty;

            // The correlation id should be present in the HttpContent. We add one in, if missing.
            var correlationId = Request.Headers.GetCorrelationId();

            // Here we're demonstrating new'ing up a message n times and publishing each one
            // as a load/perf test for SDK + AWS
            for (var i = 0; i < request.NumberOfTimesToSend; i++)
            {
                // Build up a new message (metadata + content)
                var debugData = new DebugData(Program.ServiceName, BuildInfo.Version)
                {
                    // These 2 are optional, nor required. Although schema version at least is desired.
                    // Model/object/class version
                    MessageSchemaVersion = request.SchemaVersion,
                    // Any additional pertinent info you want to provide
                    AdditionalInfo = request.AdditionalDebugInfo
                };
                var metadata = new MessageMetadata(correlationId, debugData)
                {
                    AccountId = (long) account,
                    InteractionGuid = interactionGuid
                };
                var newMessage = new Message(request.MessageContent, metadata);
                await _messagePublisher.PublishAsync(newMessage);
            }

            return Ok();
        }
    }
}
