namespace Ollama
{
    public enum KeepAlive
    {
        UnloadImmediately = 0,
        ThirtySeconds = 30,
        FiveMinutes = 300,
        LoadForever = -1
    }
}
