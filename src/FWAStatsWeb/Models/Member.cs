using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class Member
{
    [ForeignKey("Clan")]
    [DataMember]
    [StringLength(15)]
    public string ClanTag { get; set; }

    [Key]
    [DataMember]
    [StringLength(15)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Tag { get; set; }

    [DataMember]
    [StringLength(50)]
    public string Name { get; set; }

    [DataMember]
    public int ExpLevel { get; set; }

    [DataMember]
    public int Trophies { get; set; }

    [DataMember]
    [StringLength(15)]
    public string Role { get; set; }

    [DataMember]
    public int ClanRank { get; set; }

    [DataMember]
    public int Donations { get; set; }

    [DataMember]
    public int DonationsReceived { get; set; }

    public virtual Clan Clan { get; set; }

    [DataMember]
    private League League { get; set; }

    [DataMember]
    private League LeagueTier { get; set; }

    [StringLength(30)]
    public string LeagueName { get; set; }

    [StringLength(150)]
    public string BadgeUrl { get; set; }

    [NotMapped]
    public int TownHallLevel { get; set; }

    public void FixData(string clanTag)
    {
        ClanTag = clanTag;

        if (LeagueTier != null)
        {
            LeagueName = LeagueTier.Name;

            if (LeagueTier.IconUrls != null)
            {
                BadgeUrl = LeagueTier.IconUrls.Small;
            }
        }
    }

    public string LinkID
    {
        get
        {
            return Logic.Utils.TagToLinkId(Tag);
        }
    }
}
