using System.ComponentModel.DataAnnotations;

namespace MyEstore.Models;

public class WishlistItemModel
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserModel? User { get; set; }

    public int ProductId { get; set; }
    public ProductModel? Product { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
