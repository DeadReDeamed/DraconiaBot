namespace DiscordBot.Database.Profile
{
    public class Player : Entity
    {
        public ulong DiscordId { get; set; }
        public ulong GuildId { get; set; }
        public int Xp { get; set; }

        public Player(int id) : base(id) 
        {
            
        }

        public Player(int id, ulong DiscordId, ulong GuildId, int Xp) : base(id)
        {
            this.DiscordId = DiscordId;
            this.GuildId = GuildId; 
            this.Xp = Xp; 
        }
    }
}
