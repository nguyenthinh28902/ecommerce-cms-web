using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Order.Request;
using Ecom.Cms.Web.Shared.Models.Order.ViewModel.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Application.Order.Interfaces
{
    public interface IOrderService
    {
        Task<Result<List<OrderSummaryViewModel>>> GetOrderSummaryViewModels();
        Task<Result<OrderViewModel>> GetOrderById(OrderDetailRequest request);
    }
}
