namespace QLKhoHang.Models
{
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static ApiResult Success(string message = "Thành công")
            => new ApiResult { IsSuccess = true, Message = message };

        public static ApiResult Failure(string message = "Thất bại")
            => new ApiResult { IsSuccess = false, Message = message };
    }

}
