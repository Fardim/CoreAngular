using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        public int PageNumber { get; set; } = 1;

        private int MaxPageSize = 50;
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > this.MaxPageSize) ? MaxPageSize : value;
        }

        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "Unread";
    }
}
