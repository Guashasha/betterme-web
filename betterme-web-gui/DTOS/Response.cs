namespace betterme_web_gui.DTOS
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? RedirectPage { get; set; }
    }
}