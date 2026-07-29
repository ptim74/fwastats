using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class UserDetail
{
    [Key]
    [DataMember]
    [StringLength(50)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime? LastLogin { get; set; }
}
