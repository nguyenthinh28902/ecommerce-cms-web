using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Dashboard
{
    public class SummaryMetrics
    {
        public SummaryMetrics() { }
        public string Title { get; set; }
        public int value { get; set; }
        public string Group { get; set; }
        public string TitleGroup { get; set; }

        // Thêm các thuộc tính bổ trợ (Optional)
        public string Icon { get; set; } = "bi-box";
        public string CssClass { get; set; } = "bg-primary";
    }
}
