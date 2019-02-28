using AccountSystem.Core;
using AccountSystem.Models.SMS;
using AccountSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using qcloudsms_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Controllers
{
    [Route("api/Verify")]
    public class VerifyController : ControllerBase
    {
        private readonly ISMSSender _smsSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VerifyController> _logger;

        public VerifyController(ISMSSender smsSender,
            IConfiguration configuration,
            ILogger<VerifyController> logger)
        {
            _smsSender = smsSender;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("Sms")]
        public async Task<IActionResult> SendSMS(string phoneNumber, VerifyType type)
        {
            string random = (new Random().Next() % 900000L + 100000L).ToString();
            string exp = _configuration.GetSection("SMSSettings")["Exp"] ?? "30";
            int templateId = Convert.ToInt32(_configuration.GetSection("SMSSettings")["VerifyTemplateId"]);
            var result = await _smsSender.SendVerifySMSAsync(phoneNumber, templateId, new[] { random, exp }, type);
            if(result.result == 0)
            {
                return Ok(new ApiResult() { Data = null, Error_Code = 0, Msg = "验证短信发送成功", Request = "Post /api/verify/sms" });
            }
            else if(result.result == 1016)
            {
                _logger.LogError(result.result.ToString(), result);
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "手机号格式错误", Request = "Post /api/verify/sms" });
            }
            else if((result.result >= 1022 && result.result <= 1026) || result.result == 1013)
            {
                _logger.LogError(result.result.ToString(), result);
                return BadRequest(new ApiResult() { Data = null, Error_Code = 400, Msg = "单个手机号下发短信条数超过设定的上限，请联系客服", Request = "Post /api/verify/sms" });
            }
            else if(result.result == 1008)
            {
                return await SendSMS(phoneNumber, type);                
            }
            else
            {
                _logger.LogError(result.result.ToString(), result);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }        
    }
}
