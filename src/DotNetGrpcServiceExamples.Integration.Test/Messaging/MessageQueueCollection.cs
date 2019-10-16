using System.Threading.Tasks;
using NewVoiceMedia.Messaging.Client;
using Xunit;
using Xunit.Abstractions;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Messaging
{
    [CollectionDefinition("MessageQueue")]
    public class MessageQueueCollection : ICollectionFixture<MessageQueueFixture>
    {
    }

    public class MessageQueueFixture : IAsyncLifetime
    {
        public TestQueue Queue;
        private readonly IMessageSink _diagnosticMessageSink;

        public MessageQueueFixture(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public async Task InitializeAsync()
        {
            Queue = new TestQueue(
                new XunitDiagnosticLogger<MessagePublisher>(_diagnosticMessageSink),
                new XunitDiagnosticLogger<MessageConsumer>(_diagnosticMessageSink));
            await Queue.Start();
        }

        public async Task DisposeAsync()
        {
            await Queue.Stop();
        }
    }
}
