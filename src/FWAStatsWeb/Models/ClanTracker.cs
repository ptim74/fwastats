using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class ClanTracker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(30)]
        public String Id { get; set; }

        [StringLength(15)]
        public string ClanTag { get; set; }

        [StringLength(30)]
        public String ClientId { get; set; }

        [StringLength(100)]
        public string Token { get; set; }

        [StringLength(5000)]
        private string Data { get; set; }

        public ICollection<Donation> GetDonations()
        {
            try
            {
                var ret = JsonConvert.DeserializeObject<ICollection<Donation>>(Data);
                if (ret == null)
                    ret = new List<Donation>();
                return ret;
            }
            catch (Exception)
            {
                return new List<Donation>();
            }
        }

        public void SetDonations(ICollection<Donation> value)
        {
            Data = JsonConvert.SerializeObject(value);
        }
    }
}
