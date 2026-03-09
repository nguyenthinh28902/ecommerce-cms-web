using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Order.Request
{
    public class OrderDetailRequest
    {
        [Required(ErrorMessage = "Order id không được bỏ trống")]
        public int Id { get; set; }
    }
}
