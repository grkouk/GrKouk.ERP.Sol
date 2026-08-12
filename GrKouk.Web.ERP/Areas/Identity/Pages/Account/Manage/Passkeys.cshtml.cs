#nullable disable

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Areas.Identity.Pages.Account.Manage
{
    public class PasskeysModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public PasskeysModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<PasskeyViewModel> Passkeys { get; private set; } = new List<PasskeyViewModel>();

        [TempData]
        public string StatusMessage { get; set; }

        public class PasskeyViewModel
        {
            public string CredentialId { get; init; }
            public string Name { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
            public bool IsBackedUp { get; init; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string credentialId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!TryDecodeCredentialId(credentialId, out var rawCredentialId))
            {
                StatusMessage = "Error: that passkey could not be identified.";
                return RedirectToPage();
            }

            var result = await _userManager.RemovePasskeyAsync(user, rawCredentialId);
            StatusMessage = result.Succeeded
                ? "Passkey removed."
                : "Error: " + string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToPage();
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var passkeys = await _userManager.GetPasskeysAsync(user);

            Passkeys = passkeys
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PasskeyViewModel
                {
                    CredentialId = Base64Url.EncodeToString(p.CredentialId),
                    Name = string.IsNullOrWhiteSpace(p.Name) ? "Passkey" : p.Name,
                    CreatedAt = p.CreatedAt,
                    IsBackedUp = p.IsBackedUp
                })
                .ToList();
        }

        private static bool TryDecodeCredentialId(string value, out byte[] credentialId)
        {
            credentialId = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                credentialId = Base64Url.DecodeFromChars(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
