using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBot.Database.Entities
{
    public class BotOptions
    {
        public string Token { get; set; } = "";
        public string DbPath { get; set; } = "ticketbot.db";
        public ulong OwnerId { get; set; }
        public string CommandSync { get; set; } = "none"; // "dev", "global" or "none"
        public ulong[] DevGuildIds { get; set; } = Array.Empty<ulong>();
    }
}
