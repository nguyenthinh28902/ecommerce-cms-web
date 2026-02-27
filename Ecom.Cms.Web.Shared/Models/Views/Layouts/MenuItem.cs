using Ecom.Cms.Web.Shared.Models.AuthWeb;

namespace Ecom.Cms.Web.Shared.Models.Views.Layouts
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public List<DepartmentCode> Policies { get; set; } = new List<DepartmentCode>(); // Quyền cần thiết để xem menu này
    }

    public static class MenuItemValue
    {
        public static List<MenuItem> GetMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem { Title = "Ngành hàng", Controller = "Category", Action = "Index", Icon = "bi-grid-3x3-gap", Policies = new List<DepartmentCode>(){ DepartmentCode.Content } },
                new MenuItem { Title = "Thương hiệu", Controller = "Brand", Action = "Index", Icon = "bi-patch-check", Policies = new List<DepartmentCode>(){ DepartmentCode.Content }},
                new MenuItem { Title = "Sản phẩm", Controller = "Product", Action = "Index", Icon = "bi-box-seam",Policies = new List<DepartmentCode>(){ DepartmentCode.Content } },
            };
        }
    }
}
