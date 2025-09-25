using System.ComponentModel;

namespace BLL.Models
{
    public enum RoleType
    {
        [Description("User")]
        User,
        [Description("Moderator")]
        Moderator,
        [Description("Administrator")]
        Administrator
    }
}