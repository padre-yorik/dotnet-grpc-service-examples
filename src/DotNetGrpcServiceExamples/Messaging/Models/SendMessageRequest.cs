using System;
using NewVoiceMedia.OtherExample.Messages;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models
{
    public class SendMessageRequest
    {
        /// <summary>
        /// SchemaVersion represents a version of your class / object / model version. This does not mean the MetaData version.
        /// So in this example, it's the version of "MessageExample"
        /// </summary>
        /// <value>The schema version.</value>
        public string SchemaVersion { get { return "1.0.0"; } }

        /// <summary>
        /// InteractionGUID is an optional message metadata field where you can reference an interactions' ID if that has meaning
        /// </summary>
        /// <value>The interaction GUID.</value>
        public Guid? InteractionGuid { get; set; }

        /// <summary>
        /// AdditionalDebugInfo is where you would include any pertinent info (not stack trace)
        /// </summary>
        /// <value>The additional debug info.</value>
        public string AdditionalDebugInfo { get; set; }

        /// <summary>
        /// The actual simulated message from another service to publish
        /// </summary>
        /// <value>A DotNetGrpcServiceExamplesBarCreatedV1 message</value>
        public OtherExampleFooCreatedV1 MessageContent { get; set; }

        /// <summary>
        /// NumberOfTimesToSend will send the message the number of times specified. The total time taken to send will be reported in the response.
        /// This is for the purpose of testing the send rate from inside of AWS.
        /// </summary>
        public int NumberOfTimesToSend { get; set; } = 1;
    }
}
