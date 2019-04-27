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
            if (userParams.Likers)
            {
                var userLikers = await this.GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Any(d => d.LikerId == u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await this.GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Any(d => d.LikeeId == u.Id));
            }
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

        private async Task<IEnumerable<Like>> GetUserLikes(int id, bool likers)
        {
            var users = await _context.Users.Include(u => u.Liker).Include(u => u.Likee).FirstOrDefaultAsync(u => u.Id == id);
            if (likers)
            {
                return users.Likee.Where(l => l.LikeeId == id);
            }
            else
            {
                return users.Liker.Where(l => l.LikerId == id);
            }
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

        public async Task<Like> GetLike(int userId, int recipientid)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientid);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.Include(m => m.Sender).ThenInclude(s => s.Photos).Include(m => m.Recipient)
                .ThenInclude(r => r.Photos).AsQueryable();
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId && m.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(m => m.IsRead == false && m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
                    break;
            }
            messages = messages.OrderByDescending(m => m.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages.Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient)
                .ThenInclude(r => r.Photos).Where(m =>
                    (m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId) ||
                    (m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)).OrderByDescending(m => m.MessageSent)
                .ToListAsync();
            return messages;
        }
    }
}
