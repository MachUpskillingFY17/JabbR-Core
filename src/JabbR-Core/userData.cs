namespace JabbR_Core
{
    // Class object to replace repo and EF models.. hard coded in Chat.cs for invoke functions
    // user data is gathered upon login/auth which populates models

    public class UserData
    {
        // members
        public string Name;
        public string Hash;
        public string Owner;
        public string Active;
        public string NoteClass;
        public string Note;
        public string FlagClass;
        public string Flag;
        public string Country;
        public string LastActive;
        public string TimeAgo;
        public bool Admin;
        public bool Afk;
                                                                  
    }
}