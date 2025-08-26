namespace WebApp.Models;
public class UserProfileDto: UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FatherName { get; set; }
    public string UserName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string ProfileImage { get; set; }
}
