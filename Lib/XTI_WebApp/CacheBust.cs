namespace XTI_WebApp
{
    public sealed class CacheBust
    {
        public CacheBust(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public string Query() => $"cacheBust={Value}";

        public override string ToString() => $"{nameof(CacheBust)} {Value}";
    }
}
