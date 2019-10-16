using System;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models;
using NewVoiceMedia.Messaging.Models;
using NewVoiceMedia.OtherExample.Messages;
using Xunit;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Messaging
{
    [Trait("Category", "Integration")]
    [Collection("MessageQueue")]
    public class MessagingTests
    {
        private readonly TestQueue _queue;
        private readonly int _accountId;

        public MessagingTests(MessageQueueFixture messageQueueFixture)
        {
            _queue = messageQueueFixture.Queue;
            _accountId = new Random().Next();
        }

        [Fact]
        public async Task ConsumeFooCreated_ProduceBarCreated()
        {
            var fooMessage = new OtherExampleFooCreatedV1
            {
                Foo = "Test Content"
            };
            var metadata = BuildMessageMetadata();
            await _queue.MessagePublisher.PublishAsync(new Message(fooMessage, metadata));

            var barMessage = await _queue.FindMessage("dotnetgrpcserviceexamples-bar-created-v1", (ulong)_accountId, "Test Content");
            barMessage.Should().NotBeNull();
            barMessage.MessageMetadata.DebugData.ProducerName.Should().Be("dotnetgrpcservice");
            barMessage.MessageMetadata.CorrelationId.Should().Be(metadata.CorrelationId);
            barMessage.MessageMetadata.AccountId.Should().Be(_accountId);

            var messageContent = JsonConvert.DeserializeObject<DotNetGrpcServiceExamplesBarCreatedV1>(barMessage.MessageContent);
            messageContent.Bar.Should().Be("Test Content");
        }

        private MessageMetadata BuildMessageMetadata()
        {
            var debugData = new DebugData("example-test", "1.0.0");
            return new MessageMetadata(Guid.NewGuid().ToString(), debugData)
            {
                AccountId = _accountId
            };
        }
    }
}
