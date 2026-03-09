using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Order.ViewModel.Order
{
    public class OrderItemSummaryViewModel
    {
        public string ProductName { get; set; } = null!;
        public string ProductMainImage { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
