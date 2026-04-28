using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class CategoryModel
{
    [Key] // Primary Key
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } =string.Empty;
    [MaxLength(300)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    //Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    //Relationships
    public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
}