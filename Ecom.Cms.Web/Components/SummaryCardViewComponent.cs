using Ecom.Cms.Web.Shared.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.Cms.Web.Components
{
    public class SummaryCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<SummaryMetrics> metric)
        {
            return View(metric);
        }
    }
}
