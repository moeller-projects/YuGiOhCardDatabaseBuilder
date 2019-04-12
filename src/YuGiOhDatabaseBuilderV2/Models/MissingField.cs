namespace YuGiOhDatabaseBuilderV2.Models
{
    public class MissingField
    {
        public string Location { get; private set; }
        public string Name { get; private set; }
        public MissingField(string location, string name)
        {
            Location = location;
            Name = name;
        }
    }
}
