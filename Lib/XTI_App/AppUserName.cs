﻿using System;
using XTI_Core;

namespace XTI_App
{
    public sealed class AppUserName : SemanticType<string>, IEquatable<AppUserName>
    {
        public static readonly AppUserName Unknown = new AppUserName("");
        public static readonly AppUserName Anon = new AppUserName("xti_anon");

        public AppUserName(string value)
            : base(value?.Trim().ToLower() ?? "", value)
        {
        }

        public override bool Equals(object obj) => base.Equals(obj);

        public bool Equals(AppUserName other) => _Equals(other);

        public override int GetHashCode() => base.GetHashCode();

    }
}
