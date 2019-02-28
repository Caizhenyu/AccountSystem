using AccountSystem.Core;
using AccountSystem.Models.Client;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "AccountAdmin")]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IResourceStore _resourceStore;
        private readonly IClientStore _clientStore;
        private readonly ILogger<ClientController> _logger;

        public ClientController(ConfigurationDbContext configurationDbContext,
            IResourceStore resourceStore,
            IClientStore clientStore,
            ILogger<ClientController> logger)
        {
            _configurationDbContext = configurationDbContext;
            _resourceStore = resourceStore;
            _clientStore = clientStore;
            _logger = logger;
        }

        [HttpPost("Scope")]
        public async Task<IActionResult> AddScope([FromBody] ClientScope clientScope)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(clientScope.Client);
            if(client == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{clientScope.Client}不存在或不可用", Request = "Post /api/client/scope" });
            }
            var scope = await _resourceStore.FindApiResourceAsync(clientScope.Scope);
            if(scope == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{clientScope.Scope}不存在或不可用", Request = "Post /api/client/scope" });
            }

            var currentClient = _configurationDbContext.Clients.Include(c => c.AllowedScopes).FirstOrDefault(c => c.ClientId == clientScope.Client);
            currentClient.AllowedScopes.Add(new IdentityServer4.EntityFramework.Entities.ClientScope() { Scope = clientScope.Scope });

            try
            {
                _configurationDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.InnerException.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = $"{clientScope.Client}添加{clientScope.Scope}成功", Request = "Post /api/client/scope" });
        }

        [HttpPost("Lifetime")]
        public async Task<IActionResult> UpdateTokenLifetime([FromBody] ClientLifetime clientLifetime)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = ModelState.Keys.FirstOrDefault() + "错误", Request = "Post /api/client/lifetime" });
            }

            var client = await _clientStore.FindEnabledClientByIdAsync(clientLifetime.Client);
            if(client == null)
            {
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = $"{clientLifetime.Client}不存在或不可用", Request = "Post /api/client/lifetime" });
            }

            if(clientLifetime.Lifetime == client.AccessTokenLifetime)
            {
                return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = $"{clientLifetime.Client}的 Token 过期时间已改为{clientLifetime.Lifetime}", Request = "Post /api/cleint/lifetime" });
            }

            client.AccessTokenLifetime = clientLifetime.Lifetime;

            try
            {
                _configurationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = $"{clientLifetime.Client}的 Token 过期时间已改为{clientLifetime.Lifetime}", Request = "Post /api/cleint/lifetime" });
        }
    }
}
