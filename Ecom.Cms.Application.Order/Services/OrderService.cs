using Ecom.Cms.Application.Order.Interfaces;
using Ecom.Cms.Application.Product.Models;
using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Order.Request;
using Ecom.Cms.Web.Shared.Models.Order.ViewModel.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Ecom.Cms.Application.Order.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderService(ILogger<OrderService> logger, HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<List<OrderSummaryViewModel>>> GetOrderSummaryViewModels()
        {
            try
            {
                var response = await _httpClient.GetAsync(ConfigApiOrderService.OrderManagerAsync);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Result<List<OrderSummaryViewModel>>>();
                    if(result == null) return Result<List<OrderSummaryViewModel>>.Failure("Không tìm được dữ liệu");
                    return result;
                }
                else
                {
                    _logger.LogError($"Lỗi khi gọi API lấy thông tin đơn hàng: {response.StatusCode}");
                    return Result<List<OrderSummaryViewModel>>.Failure("Không tìm được dữ liệu");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
                return Result<List<OrderSummaryViewModel>>.Failure("Không tìm được dữ liệu");
            }
        }

        public async Task<Result<OrderViewModel>> GetOrderById(OrderDetailRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ConfigApiOrderService.OrderDetailManagerAsync, request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Result<OrderViewModel>>();
                    if (result == null) return Result<OrderViewModel>.Failure("Không tìm được dữ liệu");
                    return result;
                }
                else
                {
                    _logger.LogError($"Lỗi khi gọi API lấy thông tin đơn hàng: {response.StatusCode}");
                     return Result<OrderViewModel>.Failure("Không tìm được dữ liệu");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
                return Result<OrderViewModel>.Failure("Không tìm được dữ liệu");
            }
        }

    }
}
