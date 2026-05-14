using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DreysStoreEcommerce.Areas.Identity.Pages.Account.Manage
{
    public class PhoneModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PhoneModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Input = new InputModel { PhoneNumber = await _userManager.GetPhoneNumberAsync(user) };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            StatusMessage = "✅ Phone number updated!";
            return RedirectToPage();
            //return RedirectToAction("Index", "Products");
        }
    }
}
