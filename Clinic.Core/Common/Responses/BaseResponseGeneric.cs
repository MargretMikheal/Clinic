using System.Collections.Generic;

namespace Clinic.Core.Common.Responses
{
    public class BaseResponse<T> : BaseResponse
    {
        public T Data { get; set; }

        public static BaseResponse<T> Success(T data, string message = null)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public new static BaseResponse<T> Fail(string message = null, List<string> errors = null)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
