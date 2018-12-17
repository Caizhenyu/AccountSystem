using AccountSystem.Data;
using AccountSystem.Models.SMS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using qcloudsms_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Services
{
    public class SMSSender : ISMSSender
    {
        private static int _appId;
        private static string _appKey;
        private static string _sign;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SMSSender> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        public SMSSender(IConfiguration configuration,
            ILogger<SMSSender> logger,
            ApplicationDbContext applicationDbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _applicationDbContext = applicationDbContext;

            _appId = Convert.ToInt32(_configuration.GetSection("SMSSettings")["AppId"]);
            _appKey = _configuration.GetSection("SMSSettings")["AppKey"];
            _sign = _configuration.GetSection("SMSSettings")["Sign"];
        }

        public Task<SmsSingleSenderResult> SendVerifySMSAsync(string phoneNumber, int templateId, string[] parameters, VerifyType type)
        {
            return Task.Run(() =>
             {
                 try
                 {
                     SmsSingleSender ssender = new SmsSingleSender(_appId, _appKey);
                     var result = ssender.sendWithParam("86", phoneNumber,
                         templateId, parameters, _sign, "", "");  // 签名参数未提供或者为空时，会使用默认签名发送短信
                     _applicationDbContext.SMSVerifyRecord.Add(new Models.SMSVerifyRecord()
                     {
                         Sid = result.sid,
                         PhoneNumber = phoneNumber,
                         Code = parameters?[0],
                         Deadline = DateTime.Now.AddMinutes(Convert.ToDouble(parameters?[1])),
                         Type = type
                     });
                     _applicationDbContext.SaveChanges();

                     return result;
                 }
                 catch (Exception ex)
                 {
                     return new SmsSingleSenderResult() { result = -1, errMsg = ex.InnerException.Message };
                 }
             });
        }
    }
}
