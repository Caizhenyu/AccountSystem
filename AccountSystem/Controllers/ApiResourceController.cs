using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountSystem.Core;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "AccountAdmin")]
    [Route("api/[controller]")]
    public class ApiResourceController : ControllerBase
    {
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IResourceStore _resourceStore;
        private readonly ILogger<ApiResourceController> _logger;

        public ApiResourceController(ConfigurationDbContext configurationDbContext,
            IResourceStore resourceStore,
            ILogger<ApiResourceController> logger)
        {
            _configurationDbContext = configurationDbContext;
            _resourceStore = resourceStore;
            _logger = logger;
        }

        [HttpPost("Claim")]
        public IActionResult UpdateApiResource([FromBody]string apiName)
        {
            var apir = _resourceStore.FindApiResourceAsync(apiName);
            var api = _configurationDbContext.ApiResources.Include(a => a.UserClaims).FirstOrDefault(a => a.Name == apiName);
            api.UserClaims = api.UserClaims ?? new List<ApiResourceClaim>();
            if(!api.UserClaims.Exists(c => c.Type == JwtClaimTypes.Name) && !apir.Result.UserClaims.Contains(JwtClaimTypes.Name.ToString()))
            {
                api.UserClaims.Add(new ApiResourceClaim() { Type = JwtClaimTypes.Name });
            }
            if (!api.UserClaims.Exists(c => c.Type == JwtClaimTypes.Role) && !apir.Result.UserClaims.Contains(JwtClaimTypes.Role.ToString()))
            {
                api.UserClaims.Add(new ApiResourceClaim() { Type = JwtClaimTypes.Role });
            }
            try
            {
                _configurationDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.InnerException.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = $"{apiName}添加Name、Role成功" });
        }
    }
}
