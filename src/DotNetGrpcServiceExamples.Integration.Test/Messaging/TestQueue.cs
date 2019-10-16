using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewVoiceMedia.Messaging;
using NewVoiceMedia.Messaging.Client;
using NewVoiceMedia.Messaging.Models;
using NewVoiceMedia.Messaging.NetCore;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Messaging
{
    public class TestQueue
    {
        public readonly MessagePublisher MessagePublisher;
        public readonly MessageConsumer MessageConsumer;
        private readonly IAmazonSQS _sqs;
        private readonly IAmazonSimpleNotificationService _sns;
        private string _queueUrl;
        private IDictionary<string, string> _subscriptions;
        private readonly MessageServiceOptions _messageOptions;
        private readonly MessageHistory _messageHistory = new MessageHistory();

        public TestQueue(ILogger<MessagePublisher> publisherLogger, ILogger<MessageConsumer> consumerLogger)
        {
            _sqs = TestConfig.Config.GetAWSOptions("AWS_SQS").CreateServiceClient<IAmazonSQS>();
            _sns = TestConfig.Config.GetAWSOptions("AWS_SNS").CreateServiceClient<IAmazonSimpleNotificationService>();
            _messageOptions = TestConfig.Config.Get<MessageServiceOptions>();
            MessagePublisher = new MessagePublisher(Options.Create(_messageOptions), publisherLogger, _sns);
            MessageConsumer = new MessageConsumer(Options.Create(_messageOptions), consumerLogger, _sqs, new NLogContext(), new List<ITypedHandler>(), _messageHistory);
        }

        public async Task Start()
        {
            await CreateQueue();
            await Task.WhenAll(
                SubscribeQueueToTopics(),
                MessageConsumer.Start());
        }

        public async Task Stop()
        {
            await Task.WhenAll(MessagePublisher.Stop(), MessageConsumer.Stop());
            await Task.WhenAll(_subscriptions.Select(s => _sns.UnsubscribeAsync(s.Value)));
            await _sqs.DeleteQueueAsync(_queueUrl);
        }

        public Task<IMessage> FindMessage(string topic, ulong accountId, string content)
        {
            return FindMessage(topic, accountId, content, TimeSpan.FromSeconds(5));
        }

        public async Task<IMessage> FindMessage(string topic, ulong accountId, string content, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < timeout)
            {
                if (_messageHistory.ContainsKey(topic))
                {
                    var msg = _messageHistory[topic]
                        .FirstOrDefault(m =>
                            m.MessageMetadata.AccountId == (long)accountId &&
                            m.MessageContent.Contains(content));
                    if (msg != null)
                    {
                        return msg;
                    }
                }

                await Task.Delay(50);
            }
            return null;
        }

        private async Task CreateQueue()
        {
            var queue = await _sqs.CreateQueueAsync($"IntegrationTest-{Guid.NewGuid()}");
            _messageOptions.SQS_URL = queue.QueueUrl;
            _queueUrl = queue.QueueUrl;
        }

        private async Task SubscribeQueueToTopics()
        {
            var allTopics = await _sns.ListTopicsAsync();
            _subscriptions = await _sns.SubscribeQueueToTopicsAsync(allTopics.Topics.Select(t => t.TopicArn).ToList(), _sqs, _queueUrl);
            var rawSubscribeTasks = _subscriptions.Select(s => _sns.SetSubscriptionAttributesAsync(s.Value, "RawMessageDelivery", "true"));
            await Task.WhenAll(rawSubscribeTasks);
        }
    }

    public class MessageHistory : Dictionary<string, List<IMessage>>, IDefaultHandler
    {
        public Task<bool> HandleMessage(IMessage message, CancellationToken cancellationToken, Func<TimeSpan, Task<bool>> extendVisibility)
        {
            var messageType = message.MessageMetadata.MessageType;
            if (!ContainsKey(messageType))
            {
                base[messageType] = new List<IMessage>();
            }
            base[messageType].Add(message);
            return Task.FromResult(true);
        }
    }
}
