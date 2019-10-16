using System;
using NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models;
using NewVoiceMedia.Messaging.SchemaGeneration;
using NewVoiceMedia.OtherExample.Messages;
using Xunit;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Messaging
{
    public class MessageSchemaTest
    {
        [Theory]
        [InlineData(typeof(DotNetGrpcServiceExamplesBarCreatedV1))]
        [InlineData(typeof(OtherExampleFooCreatedV1))]
        public void JsonSchemaMatchesMessageClass(Type messageType)
        {
            MessageSchemaTestHelper.AssertJsonSchemaMatches(messageType);
        }
    }
}
