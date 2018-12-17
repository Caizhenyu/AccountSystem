using AccountSystem.Core;
using AccountSystem.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ManagerAdmin,AccountAdmin")]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("User")]
        public async Task<IActionResult> GetUserListByRole(string role)
        {
            if(string.IsNullOrEmpty(role))
            {
                return BadRequest(new ApiResult() { data = null, error_Code = 400, msg = "请求角色不能为空", request = "Get /api/role/user" });
            }
            if(!_roleManager.RoleExistsAsync(role).Result)
            {
                return BadRequest(new ApiResult() { data = null, error_Code = 400, msg = "请求角色不存在", request = "Get /api/role/user" });
            }

            var users = await _userManager.GetUsersInRoleAsync(role);

            var phoneNumberList = users.Select(u => u.UserName);

            var PhoneNumberList = new
            {
                PhoneNumberList = phoneNumberList
            };

            return Ok(new ApiResult() { data = PhoneNumberList, error_Code = 0, msg = $"获取{role}成员列表成功", request = "Get /api/role/user" });
        }

        [HttpPost("User")]
        public async Task<IActionResult> AddRoleToUser([FromBody] RoleViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);

            if (user == null)
            {
                return BadRequest(new ApiResult() { data = null, error_Code = 400, msg = "未找到该用户", request = "Post /api/role/user" });
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }
                user = await _userManager.FindByNameAsync(user.UserName);
                var result = await _userManager.AddToRoleAsync(user, model.Role);
                if(result.Succeeded)
                {
                    return Ok(new ApiResult() { data = null, error_Code = 0, msg = $"{model.PhoneNumber}添加{model.Role}成功", request = "Post /api/role/user" });
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}
