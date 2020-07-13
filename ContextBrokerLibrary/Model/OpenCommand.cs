using System.Runtime.Serialization;

namespace ContextBrokerLibrary.Model
{
    public class OpenCommand : UpdateExistingEntityAttributesRequest
    {
        [DataMember(Name = "open", EmitDefaultValue = false)]
        public Text Open { get; set; } = new Text
        {
            Type = "command",
            Value = ""
        };
    }
}