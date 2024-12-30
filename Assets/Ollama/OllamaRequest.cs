using System;

namespace Ollama
{
    public class OllamaRequest
    {
        public string model;
        public string prompt;
        public string format;
        public bool stream;
        public int keep_alive;

        public OllamaRequest(string model, string prompt, string format, bool stream, KeepAlive keep_alive)
        {
            this.model = model;
            this.prompt = prompt;
            this.format = format;
            this.stream = stream;
            this.keep_alive = (int)keep_alive;
        }
    }
}
