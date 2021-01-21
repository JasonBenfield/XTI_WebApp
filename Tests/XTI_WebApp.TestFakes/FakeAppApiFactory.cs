﻿using XTI_App.Api;

namespace XTI_WebApp.TestFakes
{
    public sealed class FakeAppApiFactory : AppApiFactory
    {
        protected override IAppApi _Create(IAppApiUser user) => new FakeAppApi(user);
    }
}
