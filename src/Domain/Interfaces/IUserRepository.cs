using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(Guid id);
        Task<User?> GetByEmail(string email);
        Task<User> Add(User user);
        void Update(User user);
        void Delete(User user);
    }
}
