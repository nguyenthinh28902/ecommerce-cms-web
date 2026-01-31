namespace Ecom.Cms.Web.Shared.Models
{
    public class Result<T>
    {
        // Cần set; hoặc init; để thư viện Json có thể gán giá trị
        public bool IsSuccess { get; set; }
        public string? Noti { get; set; }
        public T? Data { get; set; }

        // Constructor mặc định bắt buộc phải có cho Deserialization
        public Result() { }

        protected Result(bool isSuccess, T? data, string? noti)
        {
            IsSuccess = isSuccess;
            Data = data;
            Noti = noti;
        }

        public static Result<T> Success(T data, string mess) => new(true, data, mess);
        public static Result<T> Failure(string error) => new(false, default, error);
    }
}
