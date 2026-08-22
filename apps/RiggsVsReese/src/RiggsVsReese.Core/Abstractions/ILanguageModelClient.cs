namespace RiggsVsReese.Core.Abstractions;

public interface ILanguageModelClient
{
    Task<LanguageModelMessage> SendAsync(
        IReadOnlyList<LanguageModelMessage> messages,
        bool requireToolCall,
        CancellationToken cancellationToken = default);
}
