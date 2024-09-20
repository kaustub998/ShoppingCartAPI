using EcorpAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MimeKit;
using static Azure.Core.HttpHeader;

namespace EcorpAPI.Services.RegisterLoginService
{
    public class RegisterLoginService : IRegisterLoginService
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RegisterLoginService(ShoppingCartContext shoppingCartContext, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _shoppingCartContext = shoppingCartContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel> UserRegistration(UserDetails RegistrationData)
        {
            var EmailCollision = await _shoppingCartContext.UserDetails.Where(item => item.Email.ToLower() == RegistrationData.Email.ToLower() && item.IsDeleted != true).FirstOrDefaultAsync();
            var response = new ResponseModel();

            try
            {
                if (EmailCollision == null)
                {
                    var User = new UserDetails
                    {
                        FirstName = RegistrationData.FirstName,
                        LastName = RegistrationData.LastName,
                        Email = RegistrationData.Email,
                        Gender = RegistrationData.Gender,
                        IsAdmin = RegistrationData.IsAdmin,
                        DateOfBirth = RegistrationData.DateOfBirth,
                        ContactNumber = RegistrationData.ContactNumber,
                        Image = RegistrationData.Image,
                        SaltKey = Guid.NewGuid(),
                        CreatedBy = CommonService.GetUserId(_httpContextAccessor.HttpContext),
                        CreatedOn = DateTime.UtcNow
                    };

                    User.Password = CreatePasswordHash(RegistrationData.Password, User.SaltKey);

                    _shoppingCartContext.UserDetails.Add(User);
                    await _shoppingCartContext.SaveChangesAsync();

                    response.isSuccess = true;
                    response.isError = false;
                    response.message = "";
                }
                else
                {
                    response.isSuccess = false;
                    response.isError = true;
                    response.message = "Email Already Exists";
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.isError = true;
                response.message = "Something Went Wrong";
            }

            return response;
        }

        public async Task<SessionDetails> UserLogin(LoginDetail LoginData)
        {
            SessionDetails sd = new SessionDetails();
            var user = await _shoppingCartContext.UserDetails.Where(x => x.Email.ToLower() == LoginData.Email.ToLower().Trim() && x.IsDeleted != true).FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.Password == CreatePasswordHash(LoginData.Password, user.SaltKey))
                {
                    string token = GenerateToken(user);
                    sd.userId = user.UserId;
                    sd.tokenId = token;
                    sd.firstName = user?.FirstName ?? "";
                    sd.lastName = user?.LastName ?? "";
                    sd.userRoleId = user.IsAdmin;
                    sd.IsDeactivated = user.IsDeactivated;
                }
                else
                {
                    sd.message = "Password do not match!!!";
                    sd.userId = user.UserId;
                    sd.tokenId = "";
                }
            }
            else
            {
                sd.userId = 0;
                sd.message = "User Not Found !!!";
                sd.tokenId = "";
            }

            return sd;
        }

        public static string CreatePasswordHash(string plainPassword, Guid guidSaltKey)
        {
            var guidSaltedPassword = string.Concat(plainPassword, guidSaltKey);
            return CryptoService.CreatePasswordHash(guidSaltedPassword);
        }

        private string GenerateToken(UserDetails user)
        {
            try
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:WebTokenSecret").Value));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(
                    claims: claims,
                    signingCredentials: creds,
                    expires: DateTime.Now.AddHours(2));
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return jwt;

            }
            catch (Exception ex)
            {
                throw;
            }
            return null;

        }

        public async Task<ResponseModel> ForgotPassword(ForgotPassword userDetail)
        {
            ResponseModel response = new ResponseModel();

            var resetToken = GeneratePasswordResetToken();
            var url = _config.GetValue<string>("UIBaseUrl") + "/account/resetpassword";
            var emailBody = $"To reset your password, click the following link: {url}?token={resetToken}";

            var user = await _shoppingCartContext.UserDetails.Where(item => item.Email.ToLower() == userDetail.Email.ToLower() && item.IsDeleted != true).FirstOrDefaultAsync();
            if (user != null)
            {
                user.ResetToken = resetToken;
                _shoppingCartContext.UserDetails.Update(user);
                await SendEmailAsync(userDetail.Email, "Password Reset Request", emailBody);

                response.isError = false;
                response.isSuccess = true;
                response.message = "Email Sent Successfully";
            }
            else
            {
                response.isError = true;
                response.isSuccess = false;
                response.message = "User not found";
            }
            await _shoppingCartContext.SaveChangesAsync();

            return response;
        }

        public async Task<ResponseModel> ResetPasswordAsync(AdminResetPasswordModel adminResetPasswordModel)
        {
            ResponseModel retval = new ResponseModel();
            UserDetails? user = await _shoppingCartContext.UserDetails.Where(x => x.ResetToken == adminResetPasswordModel.ResetToken).FirstOrDefaultAsync();

            if (user != null)
            {
                user.Password = CreatePasswordHash(adminResetPasswordModel.Password, user.SaltKey);
                user.ModifiedBy = user.UserId;
                user.ModifiedOn = DateTime.UtcNow;

                _shoppingCartContext.UserDetails.Update(user);
                await _shoppingCartContext.SaveChangesAsync();

                retval.errorMessage = "";
                retval.isError = false;
                retval.isSuccess = true;

                return retval;
            }
            else
            {
                retval.errorMessage = "Something Wrong";
                retval.isError = true;
                retval.isSuccess = false;
                return retval;
            }
        }

        private static string GeneratePasswordResetToken(int length = 32)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public async Task SendEmailAsync(string? Email, string? Subject, string? emailBody)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("Sender Name", "kaustub.762419@gmail.com"));
            email.To.Add(new MailboxAddress("Receiver Name", Email));

            email.Subject = Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailBody
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, false);

                smtp.Authenticate("kaustub.762419@gmail.com", "diao xshy hpwu cnqo");

                smtp.Send(email);
                smtp.Disconnect(true);
            }

        }

        public async Task<ResponseModel> ChangePassword(ChangePassword userDetail)
        {
            UserDetails? user = await _shoppingCartContext.UserDetails.Where(x => x.UserId == userDetail.UserId && x.IsDeleted != true).FirstOrDefaultAsync();
            if (user != null)
            {
                string currentPasswordHash = CreatePasswordHash(userDetail.CurrentPassword, user.SaltKey);
                if (user.Password != currentPasswordHash)
                {
                    ResponseModel errorResult = new ResponseModel
                    {
                        isError = true,
                        errorMessage = "Current Password Not Matched!"
                    };
                    return errorResult;
                }
                user.Password = CreatePasswordHash(userDetail.Password, user.SaltKey);
                user.ModifiedBy = CommonService.GetUserId(_httpContextAccessor.HttpContext);
                user.ModifiedOn = DateTime.UtcNow;
                _shoppingCartContext.UserDetails.Update(user);
            }
            await _shoppingCartContext.SaveChangesAsync();

            ResponseModel response = new ResponseModel
            {
                isSuccess = true,
                message = "Password Change Successfully"
            };
            return response;
        }
    }
}
