using Ecom.Cms.Web.Shared.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.Cms.Web.Components
{
    public class DashboardContainerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DashboardViewModel model)
        {
            // Bạn có thể thực hiện logic lọc hoặc sắp xếp tại đây nếu cần
            return View(model);
        }
    }
}
