namespace AddressBook.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int PersonId { get; set; }

        public string? Title { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string AddressLine { get; set; } = "";

        public string DisplayText =>
            $"{(string.IsNullOrWhiteSpace(Title) ? "Adres" : Title)} - {City} {District} | {AddressLine}";
    }
}
