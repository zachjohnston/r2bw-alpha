using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using r2bw.Data;

namespace r2bw.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            [Required]
            public DateTime DateOfBirth { get; set; }

            [Display(Name = "Sex (for apparel sizing only)")]
            public string Sex { get; set; }

            [Display(Name = "Size (for apparel sizing only)")]
            public string Size { get; set; }

            //public Group Group { get; set; }

            //public int GroupId { get; set; }
        }

        public async void OnGet(string returnUrl = null)
        {
            List<Group> groups = await _context.Groups.Where(g => g.Active).ToListAsync();

            ViewData["Groups"] = groups.Select(g => new SelectListItem(g.Name, g.Id.ToString()));

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new User { 
                    UserName = Input.Email, 
                    Email = Input.Email, 
                    FirstName = Input.FirstName, 
                    DateOfBirth = Input.DateOfBirth.Date,
                    LastName = Input.LastName,
                    Sex = Input.Sex,
                    Size = Input.Size,
                    WaiverSignedOn = DateTimeOffset.Now,
                    //GroupId = Input.GroupId,
                    Active = true,
                    SecurityStamp = Guid.NewGuid().ToString()};
                
                var createResult = await _userManager.CreateAsync(user, Input.Password);

                if (createResult.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add to user role
                    var authorizeResult = await _userManager.AddToRoleAsync(user, "User");

                    if (authorizeResult.Succeeded)
                    {
                        _logger.LogInformation("User added to a role.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { userId = user.Id, code },
                            protocol: Request.Scheme);

                        _logger.LogInformation("Sending confirmation email to user... ");
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        return LocalRedirect(returnUrl);
                    }
                    else 
                    {
                        string[] errors = authorizeResult.Errors.Select(e => e.Description).ToArray();
                        _logger.LogError($"User \"{user.Email}\" was not added to role \"User\".\n{String.Join('\n', errors)}");
                    }

                    foreach (var error in authorizeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            List<Group> groups = await _context.Groups.Where(g => g.Active).ToListAsync();
            ViewData["Groups"] = groups.Select(g => new SelectListItem(g.Name, g.Id.ToString()));
            return Page();
        }
    }
}
