
namespace DiscordBot.Database
{
    public abstract class Entity
    {
        public ulong Id { get; set; }

        public Entity(ulong id) 
        {
            Id = id;
        }
    }
}
