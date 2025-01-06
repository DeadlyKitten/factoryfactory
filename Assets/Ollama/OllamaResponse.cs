using System;
using Newtonsoft.Json;

namespace Ollama
{
    public class OllamaResponse
    {
        [JsonProperty("model")]
        public string Model;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("done")]
        public bool IsDone;

        [JsonProperty("done_reason")]
        public string DoneReason;

        [JsonProperty("context")]
        public int[] Context;

        [JsonProperty("total_duration")]
        public long TotalDuration;

        [JsonProperty("load_duration")]
        public long LoadDuration;

        [JsonProperty("prompt_eval_count")]
        public int PromptEvalCount;

        [JsonProperty("prompt_eval_duration")]
        public long PromptEvalDuration;

        [JsonProperty("eval_count")]
        public int EvalCount;

        [JsonProperty("eval_duration")]
        public long EvalDuration;

        [JsonProperty("response")]
        public string Response;
    }
}
