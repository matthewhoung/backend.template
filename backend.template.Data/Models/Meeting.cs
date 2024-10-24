using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.template.Data.Models;

[Table("meetings")]
public class Meeting
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title"), Required, MaxLength(50)]
    public string Title { get; set; } = null!;
}
