namespace ChatApp.Application.Interfaces.Services
{
    public interface IAIService
    {
        // Lấy phản hồi từ AI dựa trên một prompt (câu hỏi/lệnh)
        Task<string?> GetChatCompletionAsync(string userPrompt, Guid? userIdForContext = null, CancellationToken cancellationToken = default);
    }
}