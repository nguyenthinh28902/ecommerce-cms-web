using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Dashboard
{
    public static class DashboardConstants
    {
        // Text so sánh để map Icon
        public const string Brand = "Thương hiệu";
        public const string Category = "Ngành hàng";
        public const string ActiveProduct = "Sản phẩm đang kinh doanh";
        public const string PendingProduct = "Sản phẩm đang chờ mở bán";
        public const string PublishedProduct = "Sản phẩm đang mở bán";

        // Map Icon tương ứng (Bootstrap Icons)
        public static string GetIconByTitle(string title) => title switch
        {
            Brand => "bi-award",
            Category => "bi-grid-3x3-gap",
            ActiveProduct => "bi-cart-check",
            PendingProduct => "bi-hourglass-split",
            PublishedProduct => "bi-rocket-takeoff",
            _ => "bi-box" // Icon mặc định
        };
        // Map Màu sắc (Sử dụng các class của Bootstrap)
        public static string GetColorByTitle(string title) => title switch
        {
            Brand => "text-primary border-primary",
            Category => "text-info border-info",
            ActiveProduct => "text-success border-success",
            PendingProduct => "text-warning border-warning",
            PublishedProduct => "text-danger border-danger",
            _ => "text-secondary border-secondary"
        };
    }
}
