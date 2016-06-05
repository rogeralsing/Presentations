namespace ChatMessages
{
    public class Connect
    {
        public string Username { get; set; }
    }

    public class Connected
    {
        public string Message { get; set; }
    }

    public class RenameUser
    {
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
    }

    public class RenamedUser
    {
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
    }

    public class Say
    {
        public string Username { get; set; }
        public string Text { get; set; }
    }

    public class Said
    {
        public string Username { get; set; }
        public string Text { get; set; }
    }
}