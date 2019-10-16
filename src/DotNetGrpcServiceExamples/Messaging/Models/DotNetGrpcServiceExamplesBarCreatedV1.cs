using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Messaging.Models
{
    /// <summary>
    /// Example message being produced by this service, in this case the entity 'bar' being created
    /// </summary>
    [JsonObject(Title = "dotnetgrpcserviceexamples-bar-created-v1")]
    public class DotNetGrpcServiceExamplesBarCreatedV1
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
        public string Bar { get; set; }
    }
}
