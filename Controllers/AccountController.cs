using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ClimateGSR.Models;

namespace ClimateGSR.Controllers
{
    /// <summary>
    /// Controller managing user authentication, registration, two-factor authentication,
    /// external OAuth sign-in, and password recovery workflows.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Initializes a default instance of the <see cref="AccountController"/> class.
        /// Dependencies are resolved lazily via the OWIN context.
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class with explicit dependencies.
        /// </summary>
        /// <param name="userManager">The application user manager instance.</param>
        /// <param name="signInManager">The application sign-in manager instance.</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Gets the active <see cref="ApplicationSignInManager"/> instance from the OWIN context.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        /// <summary>
        /// Gets the active <see cref="ApplicationUserManager"/> instance from the OWIN context.
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        /// <summary>
        /// Displays the account login view.
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect the user after successful sign-in.</param>
        /// <returns>The login view.</returns>
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Processes user credentials and signs the user in upon validation.
        /// </summary>
        /// <param name="model">Login form submission data containing email, password, and remember-me preference.</param>
        /// <param name="returnUrl">Destination URL after authentication succeeds.</param>
        /// <returns>Redirect result or view redisplay upon failure.</returns>
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        /// <summary>
        /// Displays the view for entering two-factor authentication security codes.
        /// </summary>
        /// <param name="provider">The 2FA provider chosen (e.g. Phone or Email).</param>
        /// <param name="returnUrl">Destination URL after verification succeeds.</param>
        /// <param name="rememberMe">Whether to persist user session cookie.</param>
        /// <returns>The verify code view.</returns>
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel
                { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// Verifies a submitted two-factor authentication security code.
        /// </summary>
        /// <param name="model">Submitted verification code details.</param>
        /// <returns>Redirect result or view redisplay on invalid code.</returns>
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code,
                isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        /// <summary>
        /// Displays the user registration page.
        /// </summary>
        /// <returns>The register view.</returns>
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Creates a new application user account and automatically signs in the user upon successful registration.
        /// </summary>
        /// <param name="model">Registration view model containing user name, email, and password.</param>
        /// <returns>Redirect to Home/Index on success or redisplay of the form with errors.</returns>
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    RegisteredAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    using (var db = new ClimateGSRDbContext())
                    {
                        db.Members.Add(new Member
                        {
                            ApplicationUserId = user.Id,
                            LifetimePoints = 0,
                            GamesPlayed = 0,
                            PointsPerGame = 0.0,
                            Highscore = 0
                        });
                        
                        await db.SaveChangesAsync();
                    }
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Verifies a user's email address using a confirmation token.
        /// </summary>
        /// <param name="userId">The unique user ID.</param>
        /// <param name="code">The email confirmation token.</param>
        /// <returns>Confirmation view or error page.</returns>
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// Displays the forgot password request view.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Initiates password reset for a registered user email.
        /// </summary>
        /// <param name="model">Submitted forgot password data.</param>
        /// <returns>The confirmation view.</returns>
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Displays the confirmation view indicating that password reset instructions were sent.
        /// </summary>
        /// <returns>The forgot password confirmation view.</returns>
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Displays the reset password entry form.
        /// </summary>
        /// <param name="code">Password reset token.</param>
        /// <returns>The reset password view or error view if token is missing.</returns>
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        /// <summary>
        /// Resets the user's password using the provided security code and new password.
        /// </summary>
        /// <param name="model">Password reset details.</param>
        /// <returns>Redirect to confirmation view or redisplay of the form with errors.</returns>
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);
            return View();
        }

        /// <summary>
        /// Displays confirmation that the password was reset successfully.
        /// </summary>
        /// <returns>The reset password confirmation view.</returns>
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Challenges an external authentication provider (OAuth/OpenID) and initiates the external redirect.
        /// </summary>
        /// <param name="provider">The name of the external provider (e.g. Google, Facebook).</param>
        /// <param name="returnUrl">URL to return to after login.</param>
        /// <returns>A <see cref="ChallengeResult"/> targeting the external provider.</returns>
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider,
                Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// Displays the selection view for two-factor authentication delivery options.
        /// </summary>
        /// <param name="returnUrl">Destination URL.</param>
        /// <param name="rememberMe">Remember me preference.</param>
        /// <returns>The send code view.</returns>
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }

            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose })
                .ToList();
            return View(new SendCodeViewModel
                { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// Generates and transmits a two-factor security token using the selected provider.
        /// </summary>
        /// <param name="model">Selection details.</param>
        /// <returns>Redirect to VerifyCode view or Error view on failure.</returns>
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }

            return RedirectToAction("VerifyCode",
                new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        /// <summary>
        /// Callback action called by the external provider upon successful authentication.
        /// </summary>
        /// <param name="returnUrl">Destination URL after login.</param>
        /// <returns>Redirect to destination, 2FA challenge, or confirmation view.</returns>
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation",
                        new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        /// <summary>
        /// Confirms details for an external login account and provisions the local application user record.
        /// </summary>
        /// <param name="model">External login confirmation details.</param>
        /// <param name="returnUrl">Destination URL.</param>
        /// <returns>Redirect result or view redisplay upon failure.</returns>
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        /// <summary>
        /// Signs out the currently authenticated user by clearing the application cookie.
        /// </summary>
        /// <returns>Redirect to Home/Index.</returns>
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the error view for failed external authentication attempts.
        /// </summary>
        /// <returns>The external login failure view.</returns>
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Releases managed and unmanaged resources held by the user and sign-in managers.
        /// </summary>
        /// <param name="disposing">True if called during explicit disposal.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}
