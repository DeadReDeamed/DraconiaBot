namespace DiscordBot.Database.Profile
{
    public class Player : Entity
    {
        public ulong DiscordId { get; set; }
        public ulong GuildId { get; set; }
        public int Xp { get; set; }
        public string Name { get; set; }
        public uint Gold { get; set; }
        public Attributes attributes { get; set; }
        public Inventory inventory { get; set; }

        public Player(int id) : base(id) 
        {
            
        }

        public Player(long id, string name, ulong DiscordId, ulong GuildId, int Xp, uint Gold) : base(id)
        {
            this.DiscordId = DiscordId;
            this.GuildId = GuildId; 
            this.Xp = Xp; 
            this.Name = name;
            this.Gold = Gold;
        }
    }
}
