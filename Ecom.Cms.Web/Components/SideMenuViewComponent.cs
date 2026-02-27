using Ecom.Cms.Web.Shared.Models.Views.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.Cms.Web.Components
{
    public class SideMenuViewComponent : ViewComponent
    {
        private readonly ILogger<SideMenuViewComponent> _logger;
        public SideMenuViewComponent(ILogger<SideMenuViewComponent> logger)
        {
            _logger = logger;
        }
        public IViewComponentResult Invoke()
        {
            var visibleMenus = MenuItemValue.GetMenuItems()
                         .Where(menu => menu.Policies.Any(role => User.IsInRole(role.ToString())))
                         .ToList();
            return View(visibleMenus);
        }
    }
}
