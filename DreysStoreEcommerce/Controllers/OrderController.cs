using AutoMapper;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.ViewModels;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Controllers
{


    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public OrderController(ApplicationDbContext context, OrderService orderService, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _context = context;
            _orderService = orderService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Where(o => o.ApplicationUserId == user.Id)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            var vm = _mapper.Map<List<OrderViewModel>>(orders);
            return View(vm);
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _orderService.GetOrdersByUserAsync(user.Id);
            return View(orders);
        }
    }


}
