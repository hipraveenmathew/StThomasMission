namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class SendAbsenteeNotificationsViewModel
    {
        public int Grade { get; set; }
        public List<string> CommunicationMethods { get; set; } = new List<string>();
    }

    public class SendFeeReminderViewModel
    {
        public int StudentId { get; set; }
        public string FeeDetails { get; set; }
        public List<string> CommunicationMethods { get; set; } = new List<string>();
    }

    public class SendGroupUpdateViewModel
    {
        public string GroupName { get; set; }
        public string UpdateMessage { get; set; }
        public List<string> CommunicationMethods { get; set; } = new List<string>();
    }
}
