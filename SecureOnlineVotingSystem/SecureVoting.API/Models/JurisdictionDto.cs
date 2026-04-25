namespace SecureVoting.API.Models
{
    public class JurisdictionDto
    {
        public int JurisdictionId { get; set; }
        public string Name { get; set; }
        public string County { get; set; }
        public string City { get; set; }
        public string ZipCode
        {
            get; set;
        }
    }
}
