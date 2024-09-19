using EcorpAPI.Models;

namespace EcorpAPI.Services.AccountService
{
    public interface IAccountService
    {
        Task<List<UserDetails>> GetAllUsers();
        Task<UserDetails> GetSingleUser(int? id);
        Task<ResponseModel> EditUserAsync(UserDetails user);
        Task<ResponseModel> DeactivateUserAsync(int? user);
        Task<ResponseModel> ReactivateUserAsync(int? user);
    }
}
