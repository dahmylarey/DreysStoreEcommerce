using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DreysStoreEcommerce.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Input = new InputModel
            {
                FullName = user.FullName,
                Address = user.Address,
                Email = user.Email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.FullName = Input.FullName;
            user.Address = Input.Address;

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            await _userManager.UpdateAsync(user);
            StatusMessage = "✅ Profile updated successfully!";

            return RedirectToPage(); // refreshes same page
        }
    }
}







//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//#nullable disable

//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Text.Encodings.Web;
//using System.Threading.Tasks;
//using DreysStoreEcommerce.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace DreysStoreEcommerce.Areas.Identity.Pages.Account.Manage
//{
//    public class IndexModel : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        public IndexModel(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public string Username { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [TempData]
//        public string StatusMessage { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [BindProperty]
//        public InputModel Input { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public class InputModel
//        {
//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Phone]
//            [Display(Name = "Phone number")]
//            public string PhoneNumber { get; set; }
//        }

//        private async Task LoadAsync(ApplicationUser user)
//        {
//            var userName = await _userManager.GetUserNameAsync(user);
//            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

//            Username = userName;

//            Input = new InputModel
//            {
//                PhoneNumber = phoneNumber
//            };
//        }

//        public async Task<IActionResult> OnGetAsync()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }

//            await LoadAsync(user);
//            return Page();
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }

//            if (!ModelState.IsValid)
//            {
//                await LoadAsync(user);
//                return Page();
//            }

//            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
//            if (Input.PhoneNumber != phoneNumber)
//            {
//                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
//                if (!setPhoneResult.Succeeded)
//                {
//                    StatusMessage = "Unexpected error when trying to set phone number.";
//                    return RedirectToPage();
//                }
//            }

//            await _signInManager.RefreshSignInAsync(user);
//            StatusMessage = "Your profile has been updated";
//            return RedirectToPage();
//        }
//    }
//}
