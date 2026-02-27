using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Application.Product.Interfaces
{
    public interface IProductSummaryService
    {
        public Task<Result<DashboardViewModel>> GetProductSummaryDashboard();
    }
}
