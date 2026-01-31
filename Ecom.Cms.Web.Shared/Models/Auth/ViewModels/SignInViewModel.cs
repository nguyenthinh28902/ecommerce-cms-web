namespace Ecom.Cms.Web.Shared.Models.Auth.ViewModels
{
    public class SignInViewModel
    {
        public string ReturnUrl { get; set; } = "/";
        // Dùng để hiển thị thông báo hoặc trạng thái hệ thống từ SSR
        public string SystemMessage { get; set; } = "Hệ thống quản lý tài khoản trung tâm";
    }
}
