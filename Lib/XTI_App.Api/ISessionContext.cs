using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public interface ISessionContext
    {
        Task<IAppUser> User();
    }
}
