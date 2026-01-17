using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUpdateUser()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return null;

            if (context.Request.Headers.TryGetValue("X-Update-user", out var user))
                return user.ToString();

            return null;
        }
    }
}
