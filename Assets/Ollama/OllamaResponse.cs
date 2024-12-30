using System;

namespace Ollama
{
    public class OllamaResponse
    {
        public string model;
        public DateTime created_at;
        public bool done;
        public string done_reason;
        public int[] context;
        public long total_duration;
        public long load_duration;
        public int prompt_eval_count;
        public long prompt_eval_duration;
        public int eval_count;
        public long eval_duration;
        public string response;
    }
}
