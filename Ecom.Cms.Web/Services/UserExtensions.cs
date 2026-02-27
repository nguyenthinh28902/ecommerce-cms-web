using System.Security.Claims;

namespace Ecom.Cms.Web.Services
{
    public static class UserExtensions
    {
        public static List<string> GetRoles(this ClaimsPrincipal user)
        {
            // Trả về list rỗng nếu user null để tránh lỗi NullReferenceException
            return user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
        }

        public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            var userRoles = user.GetRoles();
            // Kiểm tra xem User có ít nhất một trong các role truyền vào không
            return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
        }
    }
}
