using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.User;

namespace Ecom.Cms.Web.Shared.Interfaces.User
{
    public interface IUserInformation
    {
        public Task<Result<UserInforDto>> GetUserInfoAsync(string AccessToken);
    }
}
