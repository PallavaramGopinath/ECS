using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class UserRepository :Repository<Users>, IUserRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public UserRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddUser(Users users)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                //maxId = await CSISContext.Users.MaxAsync(x => x.id);
                maxId = await CSISContext.Users.AnyAsync() ? await CSISContext.Users.MaxAsync(x => x.Id) : 11010000000;

                maxId++;
                users.Id = maxId;
                await AddAsync(users);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! User registration not saved");
            }
            return result;
        }

        public async Task<bool> EditUser(Users users)
        {
            bool result = false;
            try
            {
                await EditAsync(users);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! User modifiction failed");
            }
            return result;
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(); // Generates a unique key
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash the password
            passwordSalt = hmac.Key; // Store this salt
        }

        public  bool VerifyPassWord(string enteredPassword, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt); // Use stored salt
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
            return computedHash.SequenceEqual(storedHash); // Compare with stored hash
        }

        public Users? GetUserByUsername(string username)
        {
            var user = CSISContext.Users.Where(x => x.Username == username).FirstOrDefault();
            return user;
        }

        public string GetUserRoles(decimal userId)
        {
            var user = CSISContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            return user.Role;
        }
    }
}
