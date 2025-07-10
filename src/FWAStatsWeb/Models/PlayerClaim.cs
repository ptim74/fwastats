using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class PlayerClaim
{
    [Key]
    [DataMember]
    [StringLength(15)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Tag { get; set; }

    [DataMember]
    [StringLength(50)]
    public string UserId { get; set; }
}
