namespace Koala.Yedpa.Core.Dtos
{
    public class ResponseDto
    {
        public string? Message { get; protected set; }
        public bool IsSuccess { get; protected set; }
        public int StatusCode { get; protected set; }

        public ErrorDto Errors { get; protected set; }

        public static ResponseDto Success(int statusCode, string message)
        {
            return new ResponseDto { StatusCode = statusCode, IsSuccess = true, Message = message };
        }

        public static ResponseDto Fail(int statusCode, string message, ErrorDto error)
        {
            return new ResponseDto { StatusCode = statusCode, IsSuccess = false, Message = message, Errors = error };
        }
        public static ResponseDto Fail(int statusCode, string message, List<string> errors, bool isShow)
        {
            var error = new ErrorDto();
            error.Errors = errors;
            error.IsShow = isShow;
            return new ResponseDto { StatusCode = statusCode, IsSuccess = false, Message = message, Errors = error };
        }

        public static ResponseDto Fail(int statusCode, string message, string error, bool isShow)
        {
            return new ResponseDto { Errors = new ErrorDto(error, isShow), IsSuccess = false, Message = message, StatusCode = statusCode };
        }
    }
    public class ResponseDto<T> : ResponseDto
    {
        public T Data { get; private set; }

        public static ResponseDto<T> SuccessData(int statusCode, string message, T data)
        {
            return new ResponseDto<T> { Data = data, Message = message, IsSuccess = true, StatusCode = statusCode };
        }

        public static ResponseDto<T> FailData(int statusCode, string message, string error, bool isShow)
        {
            return new ResponseDto<T> { Errors = new ErrorDto(error, isShow), IsSuccess = false, Message = message, StatusCode = statusCode };
        }

        public static ResponseDto<T> FailData(int statusCode, string message, List<string> error, bool isShow)
        {
            return new ResponseDto<T> { Errors = new ErrorDto(error, isShow), IsSuccess = false, Message = message, StatusCode = statusCode };
        }
    }
    public class ResponseListDto<T>
    {
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public int RecordsShow { get; set; }
        public string Message { get; protected set; }
        public bool IsSuccess { get; protected set; }

        public int StatusCode { get; protected set; }

        public ErrorDto Errors { get; protected set; }
        public T Data { get; set; }

        public static ResponseListDto<T> SuccessData(int statusCode, string message, T data, int RecordsTotal, int RecordsFiltered, int RecordsShow)
        {
            return new ResponseListDto<T> { Data = data, Message = message, IsSuccess = true, StatusCode = statusCode, RecordsTotal = RecordsTotal, RecordsFiltered = RecordsFiltered, RecordsShow = RecordsShow };
        }

        public static ResponseListDto<T> FailData(int statusCode, string message, string error, bool isShow)
        {
            return new ResponseListDto<T> { Errors = new ErrorDto(error, isShow), IsSuccess = false, Message = message, StatusCode = statusCode };
        }

        public static ResponseListDto<T> FailData(int statusCode, string message, List<string> error, bool isShow)
        {
            return new ResponseListDto<T> { Errors = new ErrorDto(error, isShow), IsSuccess = false, Message = message, StatusCode = statusCode };
        }
    }
}
