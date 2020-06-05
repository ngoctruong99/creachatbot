


using Microsoft.Bot.Schema;

namespace MultiTurnPromptBot
{
    public class MovieProfile
    {
        public string MovieType { get; set; }
   
        public string PhoneNumber { get; set; }
        public string Name { get; set; }

        public int Age { get; set; }
        public Attachment Picture { get; set; }

    }
}
