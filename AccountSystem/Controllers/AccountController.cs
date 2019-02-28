using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AccountSystem.Core;
using AccountSystem.Data;
using AccountSystem.Models.Account;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AccountSystem.Controllers
{

    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;

        public AccountController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            IMapper mapper,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if(!ModelState.IsValid)
            {
                var Tip = new
                {
                    Tip = ModelState.Keys.FirstOrDefault() + "错误"
                };
                return BadRequest(new ApiResult() { Data = Tip, Error_Code = 400, Msg = "参数错误", Request = "Post /api/account/login" });
            }
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync(model.Authority);
            if (disco.IsError)
            {
                return BadRequest(new ApiResult() { Data = disco.Error, Error_Code = 400, Msg = "参数错误", Request = "Post /api/account/login" });
            }
            _logger.LogInformation("discovery disco");

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if(user == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{model.PhoneNumber}未注册", Request = "Post /api/account/login" });
            }
            var tokenClient = new TokenClient(disco.TokenEndpoint, model.ClientId, model.ClientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(user.UserName, model.Password, model.Scope);
           
            if (tokenResponse.IsError)
            {
                _logger.LogInformation("token error" + tokenResponse.Error);
            }

            if(string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "用户名或密码错误", Request = "Post /api/account/login" });
            }
            else
            {
                var accessToken = new
                {
                    AccessToken = tokenResponse.TokenType + " " + tokenResponse.AccessToken
                };
                return Ok(new ApiResult() { Data = accessToken, Error_Code = 0, Msg = "认证成功", Request = "Post /api/account/login" });
            }            
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = ModelState.Keys.FirstOrDefault() + "错误", Request = "Post /api/account/register" });
            }

            var oldUser = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (oldUser != null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{model.PhoneNumber}已经注册", Request = "Post /api/account/register" });
            }

            var verifyRecord = _applicationDbContext.SMSVerifyRecord.OrderByDescending(r => r.Id).FirstOrDefault(v => v.PhoneNumber == model.PhoneNumber && v.Type == model.Type && v.Valid);
            if (verifyRecord == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{model.PhoneNumber}未获取验证码", Request = "Post /api/account/register" });
            }
            if (verifyRecord.Code != model.VerifyCode)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "验证码已失效，请重新获取", Request = "Post /api/account/register" });
            }
            if (verifyRecord.Deadline < DateTime.Now)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "验证码已过期，请重新获取", Request = "Post /api/account/register" });
            }

            var newUser = new IdentityUser()
            {
                UserName = model.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if(!result.Succeeded)
            {
                var Tip = new
                {
                    Tip = result.Errors
                };
                return BadRequest(new ApiResult() { Data = Tip, Error_Code = 400, Msg = "注册失败", Request = "Post /api/account/register" });
            }
            if(!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }
                newUser = await _userManager.FindByNameAsync(newUser.UserName);
                await _userManager.AddToRoleAsync(newUser, model.Role);
            }

            verifyRecord.Valid = false;
            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.InnerException.Message);
            }

            return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = "注册成功", Request = "Post /api/account/register"});
        }

        [AllowAnonymous]
        [HttpPost("Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = ModelState.Keys.FirstOrDefault() + "错误", Request = "Post /api/account/password" });
            }

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{model.PhoneNumber}未注册", Request = "Post /api/account/password" });
            }

            var verifyRecord = _applicationDbContext.SMSVerifyRecord.OrderByDescending(r => r.Id).FirstOrDefault(v => v.PhoneNumber == model.PhoneNumber && v.Type == model.Type && v.Valid);
            if (verifyRecord == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{model.PhoneNumber}未获取验证码", Request = "Post /api/account/password" });
            }
            if (verifyRecord.Code != model.VerifyCode)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "验证码已失效，请重新获取", Request = "Post /api/account/password" });
            }
            if (verifyRecord.Deadline < DateTime.Now)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "验证码已过期，请重新获取", Request = "Post /api/account/password" });
            }

            verifyRecord.Valid = false;
            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.Message);
            }

            string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, model.Password);

            if (result.Succeeded)
            {
                return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = $"{model.PhoneNumber}修改密码成功", Request = "Post /api/account/password" });
            }
            else
            {
                _logger.LogError($"{model.PhoneNumber}修改密码失败", result.Errors);
                return BadRequest(new ApiResult() { Data = result.Errors, Error_Code = 0, Msg = $"{model.PhoneNumber}修改密码失败", Request = "Post /api/account/password" });
            }
        }
    }
}