using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models;
using NewVoiceMedia.OtherExample.Messages;
using Xunit;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Controllers
{
    [Trait("Category", "Integration")]
    public class MessagingControllerTests
    {
        [Fact]
        public async Task PublishMessageMinimal()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, "3"))
            {
                var response = await client.PostAsJsonAsync("messaging/publishmessageminimal", new OtherExampleFooCreatedV1 { Foo = "test content" });
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task PublishMessage()
        {
            var user = new ApiClient.User();
            using (var client = await ApiClient.CreateHttpClient(user, "3"))
            {
                var response = await client.PostAsJsonAsync(
                    "messaging/publish",
                    new SendMessageRequest
                    {
                        MessageContent = new OtherExampleFooCreatedV1 { Foo = "test content" },
                        InteractionGuid = Guid.NewGuid(),
                        AdditionalDebugInfo = "test debug info",
                        NumberOfTimesToSend = 3
                    });
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}