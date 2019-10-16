using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace NewVoiceMedia.OtherExample.Messages
{
    /// <summary>
    /// Example message being produced another service, includes testing properties to control how the example service will handle the message
    /// </summary>
    [JsonObject(Title = "otherexample-foo-created-v1")]
    [JsonSchemaExtensionData("x-domain", "OtherExampleService")]
    public class OtherExampleFooCreatedV1
    {
        /// <summary>
        /// SchemaVersion represents a version of your class / object / model version. This does not mean the MetaData version.
        /// So in this example, it's the version of "MessageExample"
        /// </summary>
        /// <value>The schema version.</value>
        [Required]
        public string SchemaVersion => "1.0.0";

        /// <summary>
        /// Some example content
        /// </summary>
        [Required]
        public string Foo { get; set; }

        /// <summary>
        /// SetVisiblityTimeout is where you can choose to override (set) the visibility timeout.
        /// Just for example / test purposes.
        /// </summary>
        /// <value>The set visiblity timeout.</value>
        public TimeSpan? SetVisiblityTimeout { get; set; }

        /// <summary>
        /// SimulateHeartBeat is intended to show how you could keep extending the visibiltytimeout as your process is taking
        /// a longer time than the timeout would normally be, and you want to keep poking it to keep it held by your processing
        /// instance. Requires SetVisiblityTimeout to be set
        /// Just for example / test purposes.
        /// </summary>
        /// <value>The simulate heart beat.</value>
        public bool? SimulateHeartBeat { get; set; }

        /// <summary>
        /// Determines whether the messaging handler should consider the outcome of the message to be a success of failure.
        /// Just for example / test purposes.
        /// </summary>
        /// <value>True if the handler should consider the message correctly handled.</value>
        public bool? IsToTreatAsSuccessfulOutcome { get; set; }
    }
}
