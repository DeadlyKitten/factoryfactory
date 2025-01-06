using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ollama;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptBuilder : ExitableMonobehaviour
{
    [SerializeField]
    private string _productNameOverride = String.Empty;

    public List<PromptSO> prompts;

    public bool IsNextScriptUserRequest { get; private set; }
    public int minSteps = 5;
    public int maxSteps = 8;

    public async Task<MainScript> GenerateNewScript(CancellationToken cancellationToken)
    {
        Debug.Log("Generating new script!");
        MainScript ms = gameObject.AddComponent<MainScript>();

        ms.numStepsToGenerate = UnityEngine.Random.Range(minSteps, maxSteps + 1);
        ms.productNameScriptSection = new ScriptSection(prompts[0]);

        if (!string.IsNullOrEmpty(_productNameOverride))
        {
            ms.productNameScriptSection.SetResultText(_productNameOverride);
        }
        else if (RequestsManager.Instance.ProductRequestList.TryPeek(out TwitchRequest twitchRequest))
        {
            IsNextScriptUserRequest = true;
            ms.request = twitchRequest;
            ms.productNameScriptSection.SetResultText(twitchRequest.RequestText);
        } 
        else
        {
            IsNextScriptUserRequest = false;
            Debug.LogWarning("ProductRequestList is empty! Generating new product name...");
            await GenerateProductName(ms);
        }

        if (cancellationToken.IsCancellationRequested)
            throw new AbandonScriptGenerationException("Cancellation requested!");

        await GenerateIntro(ms);
        LoadingBarManager.Instance.AppendLoadingBarPercent(11);

        if (cancellationToken.IsCancellationRequested)
            throw new AbandonScriptGenerationException("Cancellation requested!");

        await GenerateMain(ms);
        LoadingBarManager.Instance.AppendLoadingBarPercent(11);

        if (cancellationToken.IsCancellationRequested)
            throw new AbandonScriptGenerationException("Cancellation requested!");

        await GenerateOutro(ms);
        LoadingBarManager.Instance.AppendLoadingBarPercent(11);

        if (cancellationToken.IsCancellationRequested)
            throw new AbandonScriptGenerationException("Cancellation requested!");

        ms.AssembleScriptFromParts();

        return ms;
    }

    private async Task GenerateProductName(MainScript ms)
    {
        string prompt = ms.productNameScriptSection.text;

        // Generate a list of items to add to the prompt
        string randList = "";
        int numItemsToAdd = 5;

        int i = 0;

        // Random list of items
        foreach (string item in RandomizationManager.Instance.GetRandomListOfProducts(numItemsToAdd))
        {
            randList = randList + $"{i}. {item}\n";
            ++i;
        }

        // Leave an open listing at the end
        randList = randList + $"{numItemsToAdd}.\n";

        prompt = prompt + $"Here is a list of {numItemsToAdd} examples:\n" + randList;

        prompt = StringUtils.ReplaceTextInString("location", RandomizationManager.Instance.GetRandomLocation(), prompt);

        string result = "";

        try
        {
            result = await ScriptGenerator.Instance.CreateScriptCompletionAsync(prompt, ms.productNameScriptSection, Ollama.KeepAlive.ThirtySeconds);
        } catch
        {
            Debug.LogError("Failed to generate valid product name from API! Using a backup product name...");

            result = RandomizationManager.Instance.GetRandomProductPrompt();
        }
        
        result = StringUtils.Sanitize(result);
        result = StringUtils.RemoveStepIntros(result);
        result = result.Trim();

        result = Regex.Split(result, @"[\.\n]")[0];

        ms.productNameScriptSection.SetResultText(result);
    }

    private async Task GenerateIntro(MainScript ms)
    {
        string productName = ms.productNameScriptSection.text;

        ScriptSection section = await GenerateScriptSection(prompts[1], ms, "Lets jump right into it!");

        section.SetResultText("On this episode, we're talking about " + productName + ". " + section.text);
    }

    private async Task GenerateOutro(MainScript ms)
    {
        await GenerateScriptSection(prompts[3], ms, $"Thanks for watching {StringUtils.projectName}", false);
    }

    private async Task GenerateMain(MainScript ms)
    {
        string productName = ms.productNameScriptSection.text;
        Debug.Log("Generating the rest of the script using product name:" + productName);

        PromptSO prompt = prompts[2];

        await GenerateScriptSection(prompt, ms, "Actually, I'm too tired to talk about this.");
    }

    private async Task<ScriptSection> GenerateScriptSection(PromptSO prompt, MainScript ms, string fallbackText, bool keepAlive = true)
    {
        return await GenerateScriptSection(prompt.text, prompt, ms, fallbackText, keepAlive);
    }

    private async Task<ScriptSection> GenerateScriptSection(string customPromptText, PromptSO prompt, MainScript ms, string fallbackText, bool keepAlive)
    {
        if (stopGeneratingImmediately)
        {
            stopGeneratingImmediately = false;
            throw new AbandonScriptGenerationException();
        }

        ScriptSection section = new ScriptSection(prompt);

        string customPrompt = StringUtils.ReplaceTextInString("product name", ms.productNameScriptSection.text, customPromptText);
        customPrompt = StringUtils.ReplaceTextInString("num steps", "" + ms.numStepsToGenerate, customPrompt);

        string result = "";

        try
        {
            result = await ScriptGenerator.Instance.CreateScriptCompletionAsync(customPrompt, section, keepAlive ? Ollama.KeepAlive.ThirtySeconds : Ollama.KeepAlive.UnloadImmediately);
        }
        catch (Exception e)
        {
            if (e != null)
            {
                Debug.LogError(e.Message);
            }
        }

        result = StringUtils.FixNewlines(result);
        result = StringUtils.RemoveStepIntros(result);
        result = StringUtils.Sanitize(result);

        if (result.Equals(string.Empty))
        {
            result = fallbackText;
        }

        section.SetResultText(result);

        ms.scriptSections.Add(section);

        return section;
    }
}
