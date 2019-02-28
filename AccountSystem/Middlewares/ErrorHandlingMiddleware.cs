using AccountSystem.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch(NullReferenceException ex)
            {
                var statusCode = context.Response.StatusCode;
                await HandleExceptionAsync(context, statusCode, ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                var statusCode = context.Response.StatusCode;
                await HandleExceptionAsync(context, statusCode, ex.InnerException.Message);
            }
            finally
            {
                var statusCode = context.Response.StatusCode;
                var msg = "";
                switch(statusCode)
                {
                    //case 400:
                    //    msg = "参数错误";
                    //    break;
                    case 401:
                        bool isTimeOut = false;
                        var headers = context.Response.Headers.Values;
                        foreach (var value in headers)
                        {
                            var stringValue = value.FirstOrDefault();
                            if (stringValue.Contains("The token is expired"))
                            {
                                isTimeOut = true;
                                break;
                            }
                        }
                        msg = isTimeOut ? "Token 已过期" : "未验证身份";
                        statusCode = isTimeOut ? 4011 : 4010;
                        break;
                    case 403:
                        msg = "未授权";
                        break;
                    case 404:
                        msg = "未找到服务/资源";
                        break;
                    case 500:
                    case 502:
                        msg = "服务器错误";
                        break;
                    default:
                        if(statusCode > 400)
                        {
                            msg = "未知错误";
                        }
                        break;
                }
                
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    await HandleExceptionAsync(context, statusCode, msg);
                }
            }
        }
        //异常错误信息捕获，将错误信息用Json方式返回
        private static Task HandleExceptionAsync(HttpContext context, int statusCode, string msg)
        {
            var result = JsonConvert.SerializeObject(new ApiResult { Data = null, Error_Code = statusCode, Msg = msg, Request = context.Request.Method + " " + context.Request.Path },
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }
                );
            //context.Response.ContentType = "application/json;charset=utf-8";
            return context.Response.WriteAsync(result);
        }
    }
    //扩展方法
    public static class ErrorHandlingExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
