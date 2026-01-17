using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.Helpers
{
    public class ZDPayload
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public string? external_Id { get; set; }
        public DateTime exp { get; set; }

    }
}
