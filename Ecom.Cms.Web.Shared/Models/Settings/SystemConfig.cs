namespace Ecom.Cms.Web.Shared.Models.Settings
{
    public class SystemConfig
    {
        public const string ConfigSection = "SystemConfig"; // Tên node trong file Json
        public string GatewayUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string AuthScope { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }

}
