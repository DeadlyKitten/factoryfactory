using System;
using Newtonsoft.Json;

namespace Ollama
{
    public class OllamaRequest
    {
        [JsonProperty("model")]
        public string Model;

        [JsonProperty("prompt")]
        public string Prompt;

        [JsonProperty("format")]
        public string Format;

        [JsonProperty("stream")]
        public bool Stream;

        [JsonProperty("keep_alive")]
        public int KeepAlive;

        public OllamaRequest(string model, string prompt, string format, bool stream, KeepAlive keepAlive)
        {
            this.Model = model;
            this.Prompt = prompt;
            this.Format = format;
            this.Stream = stream;
            this.KeepAlive = (int)keepAlive;
        }
    }
}
