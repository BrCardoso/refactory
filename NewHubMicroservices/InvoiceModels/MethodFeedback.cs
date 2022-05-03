namespace NetCoreJobsMicroservice.Models.Method
{
    public class MethodFeedback
    {
        public bool Success { get; set; }

        public bool Exception { get; set; }

        public string Message { get; set; }

        public string MessageCode { get; set; }

        public object obj { get; set; }

        public MethodFeedback()
        {
            this.Success = true;
            this.Exception = false;
        }
    }
}