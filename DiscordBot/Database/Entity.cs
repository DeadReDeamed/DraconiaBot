
namespace DiscordBot.Database
{
    public abstract class Entity
    {
        public long Id { get; set; }

        public Entity(long id) 
        {
            Id = id;
        }
    }
}
