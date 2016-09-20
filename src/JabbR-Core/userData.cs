namespace JabbR_Core
{
    // Class object to replace repo and EF models.. hard coded in Chat.cs for invoke functions
    // user data is gathered upon login/auth which populates models

    public class UserData
    {
        // members
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Owner { get; set; }
        public string Active { get; set; }
        public string NoteClass { get; set; }
        public string Note { get; set; }
        public string FlagClass { get; set; }
        public string Flag { get; set; }
        public string Country { get; set; }
        public string LastActive { get; set; }
        public string LastActivity { get; set; } 
        public string TimeAgo { get; set; }
        public bool Admin { get; set; }
        public bool Afk { get; set; }
    }
}