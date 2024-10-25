using backend.template.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.template.Data.Models;

[Table("meeting_assets")]
public class MeetingAsset
{
    [Key]
    [Column("id")]
    public int Seq { get; set; }

    [Column("meeting_id"), Required]
    public Guid MeetingId { get; set; }

    [Column("name"), Required, MaxLength(50)]
    public string? Name { get; set; } = null!;

    [Column("asset_type", TypeName = "varchar(25)"), Required]
    public MeetingAssetType AssetType { get; set; }

    [Column("path"), Required, MaxLength(255)]
    public string Path { get; set; } = null!;
}
