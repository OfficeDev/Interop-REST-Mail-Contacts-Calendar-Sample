namespace MeetingManager.Models
{
    public class User
    {
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DisplayName))
            {
                return DisplayName;
            }
            return UserPrincipalName;
        }
    }
}
