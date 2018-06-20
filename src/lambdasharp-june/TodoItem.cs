using Newtonsoft.Json;

namespace lambdasharp_june
{
    public class TodoItem {
        public int Id {get; set;}

        [JsonProperty(Required=Required.Always)]
        public string Note {get; set;}

        public bool Completed {get; set;}
    }
}