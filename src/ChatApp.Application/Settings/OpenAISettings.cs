namespace ChatApp.Application.Settings
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = null!;
        public string? OrganizationId { get; set; } // Tùy chọn, một số API cần
        public string DefaultModel { get; set; } = "gpt-3.5-turbo"; // Model mặc định
    }
}