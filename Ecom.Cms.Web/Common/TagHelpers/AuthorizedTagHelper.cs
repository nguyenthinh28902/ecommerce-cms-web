using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ecom.Cms.Web.Common.TagHelpers
{
    // TagHelper này sẽ áp dụng cho bất kỳ thẻ nào có thuộc tính "asp-authorize" hoặc thẻ <authorized>
    [HtmlTargetElement(Attributes = "asp-roles")]
    public class AuthorizedTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizedTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Danh sách các Role được phép xem, cách nhau bằng dấu phẩy (ví dụ: "Admin,Manager")
        /// </summary>
        [HtmlAttributeName("asp-roles")]
        public string Roles { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // 1. Nếu không có User hoặc User chưa đăng nhập -> Ẩn thẻ
            if (user == null || !user.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            // 2. Nếu không yêu cầu Role cụ thể -> Cho phép hiển thị (vì đã đăng nhập)
            if (string.IsNullOrEmpty(Roles)) return;

            // 3. Kiểm tra xem User có thuộc ít nhất một trong các Role yêu cầu không
            var roleList = Roles.Split(',').Select(r => r.Trim());
            var hasRole = roleList.Any(role => user.IsInRole(role));

            if (!hasRole)
            {
                // Nếu không có quyền -> Xóa bỏ toàn bộ nội dung thẻ này khỏi HTML trả về
                output.SuppressOutput();
            }
        }
    }
}
