using System.Runtime.Serialization;

namespace ContextBrokerLibrary.Model
{
    public class CloseCommand : UpdateExistingEntityAttributesRequest
    {
        [DataMember(Name = "close", EmitDefaultValue = false)]
        public Text Close { get; set; } = new Text
        {
            Type = "command",
            Value = ""
        };
    }
}