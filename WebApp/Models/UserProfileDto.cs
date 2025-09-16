namespace WebApp.Models;
public class UserProfileDto: UserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string ProfileImage { get; set; } = string.Empty;
}
