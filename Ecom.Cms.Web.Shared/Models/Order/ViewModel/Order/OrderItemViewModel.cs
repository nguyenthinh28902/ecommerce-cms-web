using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Order.ViewModel.Order
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductMainImage { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalLineAmount { get; set; }
    }
}
