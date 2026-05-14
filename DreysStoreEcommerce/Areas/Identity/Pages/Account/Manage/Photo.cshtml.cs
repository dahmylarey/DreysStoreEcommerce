using System.IO;
using System.Threading.Tasks;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DreysStoreEcommerce.Areas.Identity.Pages.Account.Manage
{
    public class PhotoModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PhotoModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public string CurrentPhoto { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentPhoto = user.ProfilePictureUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (Upload != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{user.Id}_{Upload.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var fs = new FileStream(filePath, FileMode.Create);
                await Upload.CopyToAsync(fs);

                user.ProfilePictureUrl = "/uploads/" + fileName;
                await _userManager.UpdateAsync(user);
                StatusMessage = "✅ Profile picture updated!";
            }
            return RedirectToPage();
            //return RedirectToAction("Index", "Products");
        }
    }
}
