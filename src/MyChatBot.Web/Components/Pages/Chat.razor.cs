namespace MyChatBot.Web.Components.Pages;

public partial class Chat
{
    private readonly List<ChatMessage> _messages = [];
    private string _question = "";
    private bool _isThinking;

    private async Task SendAsync()
    {
        var question = _question.Trim();
        if (question.Length == 0 || _isThinking) return;

        _messages.Add(new ChatMessage(question, FromUser: true));
        _question = "";
        _isThinking = true;

        try
        {
            var result = await Api.AskAsync(question);
            _messages.Add(new ChatMessage(result.Answer, FromUser: false, result.Sources));
        }
        catch (HttpRequestException)
        {
            _messages.Add(new ChatMessage("Sorry, I couldn't reach the API. Is it running?", FromUser: false));
        }
        finally
        {
            _isThinking = false;
        }
    }

    private record ChatMessage(string Text, bool FromUser, List<SourceReference>? Sources = null);
}