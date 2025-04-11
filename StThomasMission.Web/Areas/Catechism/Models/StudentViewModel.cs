namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string? Group { get; set; }
        public string? StudentOrganisation { get; set; }
        public string Status { get; set; } = "Active";
    }
}