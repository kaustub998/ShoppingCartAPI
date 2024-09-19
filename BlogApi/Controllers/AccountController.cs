using EcorpAPI.Models;
using EcorpAPI.Services.AccountService;
using EcorpAPI.Services.RegisterLoginService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcorpAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IRegisterLoginService _registerLoginService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IAccountService accountService, ILogger<AccountController> logger, IRegisterLoginService registerLoginService)
        {
            _accountService = accountService;
            _logger = logger;
            _registerLoginService = registerLoginService;
        }

        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> UserLogin(LoginDetail loginDetail)
        {
            return Ok(await _registerLoginService.UserLogin(loginDetail));
        }

        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> UserRegistration(UserDetails userDetails)
        {
            return Ok(await _registerLoginService.UserRegistration(userDetails));
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordModel adminResetPasswordModel)
        {
            return Ok(await _registerLoginService.ResetPasswordAsync(adminResetPasswordModel));
        }

        [AllowAnonymous]
        [Route("ForgotPassword")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassword forgotPassword)
        {
            return Ok(await _registerLoginService.ForgotPassword(forgotPassword));
        }

        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            return Ok(await _registerLoginService.ChangePassword(changePassword));
        }

        [Route("GetAllUser")]
        [HttpGet,Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _accountService.GetAllUsers());
        }

        [Route("GetSingleUser/{userId}")]
        [HttpGet, Authorize]
        public async Task<IActionResult> GetSingleUser(int? userId)
        {
            return Ok(await _accountService.GetSingleUser(userId));
        }

        [Route("EditUser")]
        [HttpPost, Authorize]
        public async Task<IActionResult> EditUser(UserDetails user)
        {
            return Ok(await _accountService.EditUserAsync(user));
        }

        [Route("DeactivateUser/{user}")]
        [HttpGet, Authorize]
        public async Task<IActionResult> DeactivateUser(int? user)
        {
            return Ok(await _accountService.DeactivateUserAsync(user));
        }

        [Route("ReactivateUser/{user}")]
        [HttpGet]
        public async Task<IActionResult> ReactivateUser(int? user)
        {
            return Ok(await _accountService.ReactivateUserAsync(user));
        }
    }
}
