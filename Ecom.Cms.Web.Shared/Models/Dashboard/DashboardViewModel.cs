using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Dashboard
{
    public class DashboardViewModel
    {
        public DashboardViewModel() { } 
        public string Title { get; set; }
        public List<SummaryMetrics> SummaryMetrics { get; set; } = new List<SummaryMetrics>();
    }
}
