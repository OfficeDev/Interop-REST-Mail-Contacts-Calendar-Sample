namespace MeetingManager.Models
{
    public class EmailAddress
    {
        public string Address { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return Address;
        }
    }
}
