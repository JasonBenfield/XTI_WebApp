using System.Text;

namespace XTI_WebAppClient
{
    public sealed class ErrorModel
    {
        public string Message { get; set; }
        public string Source { get; set; }

        public string Format()
        {
            var formatted = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Source))
            {
                formatted.Append(Source);
            }
            if (!string.IsNullOrWhiteSpace(Message))
            {
                if (formatted.Length > 0)
                {
                    formatted.Append(": ");
                }
                formatted.Append(Message);
            }
            return formatted.ToString();
        }

        public override string ToString() => $"{nameof(ErrorModel)} {Format()}";
    }
}
