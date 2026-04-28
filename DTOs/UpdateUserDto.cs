namespace MyEstore.DTOs;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PinCode { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public int? RoleId { get; set; }
    // NOT exposed in the API — set server-side by the controller based on the caller's actual role
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsAdminAction { get; set; }
}
