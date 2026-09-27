using System.Collections.Generic;

namespace Clinic.Core.Common.Responses
{
    public class BaseResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ErrorId { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse()
        {
            Errors = new List<string>();
        }

        public static BaseResponse Success(string message = null)
        {
            return new BaseResponse { IsSuccess = true, Message = message };
        }

        public static BaseResponse Fail(string message = null, List<string> errors = null)
        {
            return new BaseResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
