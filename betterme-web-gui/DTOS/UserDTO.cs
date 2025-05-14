namespace BetterMe.WebGui.Dtos;
public class UserDto
{
    public string Id        { get; set; } = default!;        // maps _id
    public string Username  { get; set; } = default!;
    public string Email     { get; set; } = default!;
    public string Usertype  { get; set; } = default!;
    public string? Name     { get; set; }
    public DateTime? Birthday { get; set; }
}