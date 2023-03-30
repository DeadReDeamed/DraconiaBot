using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draconia_bot
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var bot = new Bot();

            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
