using Ecom.Cms.Application.Order.Interfaces;
using Ecom.Cms.Web.Shared.Models.Order.Request;
using Ecom.Cms.Web.Shared.Models.Order.ViewModel.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.Cms.Web.Controllers
{

    [Authorize]
    [Route("quan-ly/don-hang")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("danh-sach")]
        public async Task<IActionResult> Index()
        {
            var result = await _orderService.GetOrderSummaryViewModels();
            // 1. Chỉ comment dòng quan trọng: Trả về View kèm theo danh sách đơn hàng nếu thành công
            if (result.IsSuccess) return View(result.Data);

            ViewBag.ErrorMessage = result.Noti;
            return View(new List<OrderSummaryViewModel>());
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _orderService.GetOrderById(new OrderDetailRequest { Id = id });
            // 2. Chỉ comment dòng quan trọng: Nếu không tìm thấy đơn hàng, quay lại danh sách kèm thông báo
            if (!result.IsSuccess) return RedirectToAction(nameof(Index));

            return View(result.Data);
        }
    }
}
