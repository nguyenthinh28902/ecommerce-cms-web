using System.Text.Json.Serialization;

namespace Ecom.Cms.Web.Shared.Models.Auth.Models
{
    public class TokenResponseDto
    {
        public TokenResponseDto() { }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")] // Nên thêm trường này vì OIDC luôn trả về "Bearer"
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("is_logged")]
        public bool IsLogged { get; set; }
    }
}
