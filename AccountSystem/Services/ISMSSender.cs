using AccountSystem.Models.SMS;
using qcloudsms_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Services
{
    public interface ISMSSender
    {
        Task<SmsSingleSenderResult> SendVerifySMSAsync(string phoneNumber, int templateId, string[] parameters, VerifyType type);
    }
}
