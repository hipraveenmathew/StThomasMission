using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum MessageType
    {
        [Display(Name = "Announcement")]
        Announcement,
        [Display(Name = "Notification")]
        Notification,
        [Display(Name = "Absentee Notification")]
        AbsenteeNotification,
        [Display(Name = "Fee Reminder")]
        FeeReminder,
        [Display(Name = "Group Update")]
        GroupUpdate
    }
}