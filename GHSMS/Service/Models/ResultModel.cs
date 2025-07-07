using System.Net;

namespace Service.Models
{
    public class ResultModel
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ResultModel Success(object? data = null, string message = "Operation successful")
        {
            return new ResultModel
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = message,
                Data = data
            };
        }

        public static ResultModel Created(object? data = null, string message = "Resource created successfully")
        {
            return new ResultModel
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.Created,
                Message = message,
                Data = data
            };
        }

        public static ResultModel NotFound(string message = "Resource not found")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.NotFound,
                Message = message,
                Data = null
            };
        }

        public static ResultModel BadRequest(string message = "Bad request")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.BadRequest,
                Message = message,
                Data = null
            };
        }

        public static ResultModel Unauthorized(string message = "Unauthorized access")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.Unauthorized,
                Message = message,
                Data = null
            };
        }

        public static ResultModel Forbidden(string message = "Access forbidden")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.Forbidden,
                Message = message,
                Data = null
            };
        }

        public static ResultModel InternalServerError(string message = "Internal server error")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.InternalServerError,
                Message = message,
                Data = null
            };
        }

        public static ResultModel Conflict(string message = "Resource conflict")
        {
            return new ResultModel
            {
                IsSuccess = false,
                Code = (int)HttpStatusCode.Conflict,
                Message = message,
                Data = null
            };
        }
    }
}