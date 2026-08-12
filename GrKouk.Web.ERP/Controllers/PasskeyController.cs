using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.Controllers
{
    /// <summary>
    /// Endpoints backing the WebAuthn (passkey) browser flows.
    ///
    /// Registration uses the "PasskeyManagement" policy, which names <see cref="IdentityConstants.ApplicationScheme"/>
    /// explicitly rather than relying on the default scheme, because this app overrides the authentication defaults
    /// with a separate cookie scheme plus JWT bearer for the API. The Identity cookie is the one SignInManager
    /// actually writes.
    ///
    /// Nothing here touches the JWT bearer pipeline used by the API controllers.
    /// </summary>
    [ApiController]
    [Route("api/passkey")]
    public class PasskeyController : ControllerBase
    {
        /// <summary>
        /// Upper bound on passkeys per account. The docs call out enforcing a limit so that registration can't be
        /// used to exhaust the database.
        /// </summary>
        private const int MaxPasskeysPerUser = 10;

        private const int MaxPasskeyNameLength = 64;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public PasskeyController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ---------- Registration (attestation): requires a signed in user ----------

        [HttpPost("creation-options")]
        [Authorize(Policy = "PasskeyManagement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreationOptions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var existing = await _userManager.GetPasskeysAsync(user);
            if (existing.Count >= MaxPasskeysPerUser)
            {
                return BadRequest(new { error = $"You can register at most {MaxPasskeysPerUser} passkeys." });
            }

            var userEntity = new PasskeyUserEntity
            {
                Id = await _userManager.GetUserIdAsync(user),
                Name = await _userManager.GetUserNameAsync(user),
                DisplayName = await _userManager.GetUserNameAsync(user)
            };

            var optionsJson = await _signInManager.MakePasskeyCreationOptionsAsync(userEntity);
            return Content(optionsJson, "application/json");
        }

        [HttpPost("register")]
        [Authorize(Policy = "PasskeyManagement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] PasskeyRegistrationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.CredentialJson))
            {
                return BadRequest(new { error = "No credential was supplied." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var attestation = await _signInManager.PerformPasskeyAttestationAsync(request.CredentialJson);
            if (!attestation.Succeeded)
            {
                return BadRequest(new { error = attestation.Failure.Message });
            }

            // The attestation state carries the account the passkey was created for. Confirming it matches the
            // signed in user is what stops a passkey being attached to somebody else's account.
            var signedInUserId = await _userManager.GetUserIdAsync(user);
            if (!string.Equals(attestation.UserEntity.Id, signedInUserId, StringComparison.Ordinal))
            {
                return BadRequest(new { error = "The passkey was not created for the signed in account." });
            }

            var passkey = attestation.Passkey;
            var name = string.IsNullOrWhiteSpace(request.Name) ? "Passkey" : request.Name.Trim();
            passkey.Name = name.Length > MaxPasskeyNameLength ? name[..MaxPasskeyNameLength] : name;

            var result = await _userManager.AddOrUpdatePasskeyAsync(user, passkey);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { ok = true });
        }

        // ---------- Sign in (assertion): anonymous ----------

        [HttpPost("request-options")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOptions()
        {
            // A null user asks for a discoverable credential, so the authenticator offers whichever accounts it
            // holds for this domain and the user never types a username.
            var optionsJson = await _signInManager.MakePasskeyRequestOptionsAsync(null);
            return Content(optionsJson, "application/json");
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] PasskeyRegistrationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.CredentialJson))
            {
                return BadRequest(new { error = "No credential was supplied." });
            }

            var result = await _signInManager.PasskeySignInAsync(request.CredentialJson);

            if (result.Succeeded)
            {
                return Ok(new { ok = true });
            }

            if (result.IsLockedOut)
            {
                return BadRequest(new { error = "This account is locked out." });
            }

            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { error = "Two factor authentication is required for this account." });
            }

            return BadRequest(new { error = "That passkey was not recognised." });
        }
    }

    /// <summary>
    /// Carries the serialized credential produced by navigator.credentials.create() or .get() in the browser.
    /// </summary>
    public record PasskeyRegistrationRequest(string CredentialJson, string Name);
}
