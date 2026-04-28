using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class RoleModel
{
    [Key] // Primary Key
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    //Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    //Relationships
    public ICollection<UserModel> Users { get; set; }
    //one role can have many users, but a user can have only one role (for simplicity)
}