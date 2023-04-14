using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Database.Profile
{
    public class Attributes
    {
        public ulong CharacterId { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Constitution { get; set; }
        public short Intelligence { get; set; }

        public Attributes(ulong id, short strength, short dexterity, short constitution, short intelligence)
        {
            CharacterId = id;
            Strength = strength;
            Dexterity = dexterity;
            Constitution = constitution;
            Intelligence = intelligence;
        }

        public Attributes()
        {

        }
    }
}
