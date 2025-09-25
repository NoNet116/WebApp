namespace BLL
{
    public static class ResultExtensions
    {
        public static T? NullIfEmpty<T>(this T data)
        {
            // Проверяем, является ли объект коллекцией
            if (data is IEnumerable<object> enumerable && !enumerable.Any())
            {
                return default;
            }

            return data;
        }
    }

    public class Result<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } = new();

        public static Result<T> Ok(int statuscode, T data) =>
            new() { Success = true, StatusCode = statuscode, Data = data.NullIfEmpty() };

        public static Result<T> Fail(int statuscode, params string[] errors) =>
            new() { Success = false, StatusCode = statuscode, Errors = errors.ToList() };

        public bool DataIsNull => Data == null;
    }
}