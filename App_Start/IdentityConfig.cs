using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using ClimateGSR.Models;

namespace ClimateGSR
{
    /// <summary>
    /// Service for sending account-related emails such as confirmation links and password resets.
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// Asynchronously transmits an email message.
        /// </summary>
        /// <param name="message">The identity message containing destination, subject, and body.</param>
        /// <returns>A completed task.</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Service for sending SMS messages such as two-factor verification codes.
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// Asynchronously transmits an SMS message.
        /// </summary>
        /// <param name="message">The identity message containing destination and text body.</param>
        /// <returns>A completed task.</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Custom User Manager providing domain logic for managing user accounts, passwords, validators, and lockout rules.
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserManager"/> class with the specified user store.
        /// </summary>
        /// <param name="store">The entity user store.</param>
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        /// <summary>
        /// Factory method that creates and configures an instance of <see cref="ApplicationUserManager"/> per OWIN request.
        /// </summary>
        /// <param name="options">Identity factory configuration options.</param>
        /// <param name="context">The active OWIN context.</param>
        /// <returns>A configured <see cref="ApplicationUserManager"/> instance.</returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ClimateGSRDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    /// <summary>
    /// Custom Sign-in Manager handling user authentication, two-factor challenges, and sign-in operations.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSignInManager"/> class.
        /// </summary>
        /// <param name="userManager">The application user manager.</param>
        /// <param name="authenticationManager">The OWIN authentication manager.</param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        /// <summary>
        /// Generates the claims identity representing the signed-in user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>A task returning the user's ClaimsIdentity.</returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        /// <summary>
        /// Factory method for creating an <see cref="ApplicationSignInManager"/> instance per OWIN request.
        /// </summary>
        /// <param name="options">Factory configuration options.</param>
        /// <param name="context">The active OWIN context.</param>
        /// <returns>A configured <see cref="ApplicationSignInManager"/> instance.</returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
