using System.Threading.Tasks;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DreysStoreEcommerce.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TwoFactorModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public bool Is2faEnabled { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var enabled = await _userManager.GetTwoFactorEnabledAsync(user);
            await _userManager.SetTwoFactorEnabledAsync(user, !enabled);
            StatusMessage = enabled ? "2FA disabled." : "2FA enabled.";
            return RedirectToPage();
            //return RedirectToAction("Index", "Products");

        }
    }
}
