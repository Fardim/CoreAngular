using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data.IRepository;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data.Repository
{
    public class DatingRepository: IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).Where(u=> u.Id != userParams.UserId && u.Gender == userParams.Gender).OrderByDescending(u=> u.LastActive).AsQueryable();
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                users = users.Where(u =>
                    u.DateOfBirth.CalculateAge() >= userParams.MinAge &&
                    u.DateOfBirth.CalculateAge() <= userParams.MaxAge);
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            var pagedList = await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            return pagedList;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u=> u.Id == id);
            return user;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Photo> GetMainPhotoForUser(int id)
        {
            return await _context.Photos.Where(p => p.UserId == id).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
