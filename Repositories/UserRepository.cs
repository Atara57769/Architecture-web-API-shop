using Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiDBContext _apiDbContext;

        public UserRepository(ApiDBContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _apiDbContext.Users.ToListAsync();
        }

        public async Task<User> GetUserById(int id)
        {
            return await _apiDbContext.Users.FindAsync(id);
        }
        public async Task<IEnumerable<Order>> GetUsersOrders(int userId)
        {
            return await _apiDbContext.Orders.Include(order=>order.OrderItems).ThenInclude(item=>item.Product).Where(order=>order.UserId== userId).ToListAsync();
        }

        public async Task<User> AddUser(User newUser)
        {
            await _apiDbContext.Users.AddAsync(newUser);
            await _apiDbContext.SaveChangesAsync();
            return newUser;
        }

        public async Task UpdateUser(int id, User updateUser)
        {
            _apiDbContext.Users.Update(updateUser);
            await _apiDbContext.SaveChangesAsync();
        }

        public async Task<User> Login(string email,string password)
        {
            return await _apiDbContext.Users.Include(user=>user.Orders).FirstOrDefaultAsync(user => user.Email == email && user.Password == password);
        }

        public async Task<bool> UserWithSameEmail(string email,int id)
        {
            User userWithSameEmail;
            if (id < 0)
            {
                 userWithSameEmail = await _apiDbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
            }
            else
            {
                 userWithSameEmail = await _apiDbContext.Users.FirstOrDefaultAsync(user => user.Email == email && user.Id != id);
            }
           if(userWithSameEmail == null)
                return true;
            return false;

        }
    }
}
