﻿using Microsoft.Extensions.Options;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeOptions<T> : IOptions<T> where T : class, new()
    {
        public FakeOptions()
        {
            Value = new T();
        }

        public T Value { get; }
    }
}
