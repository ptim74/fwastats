using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class WeightResult
    {
        [Key]
        [StringLength(15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }
        public DateTime Timestamp { get; set; }
        public bool PendingResult { get; set; }

        public int TeamSize { get; set; }
        public int TH13Count { get; set; }
        public int TH12Count { get; set; }
        public int TH11Count { get; set; }
        public int TH10Count { get; set; }
        public int TH9Count { get; set; }
        public int TH8Count { get; set; }
        public int TH7Count { get; set; }
        public int THSum { get; set; }

        public int Base01 { get; set; }
        public int Base02 { get; set; }
        public int Base03 { get; set; }
        public int Base04 { get; set; }
        public int Base05 { get; set; }
        public int Base06 { get; set; }
        public int Base07 { get; set; }
        public int Base08 { get; set; }
        public int Base09 { get; set; }
        public int Base10 { get; set; }

        public int Base11 { get; set; }
        public int Base12 { get; set; }
        public int Base13 { get; set; }
        public int Base14 { get; set; }
        public int Base15 { get; set; }
        public int Base16 { get; set; }
        public int Base17 { get; set; }
        public int Base18 { get; set; }
        public int Base19 { get; set; }
        public int Base20 { get; set; }

        public int Base21 { get; set; }
        public int Base22 { get; set; }
        public int Base23 { get; set; }
        public int Base24 { get; set; }
        public int Base25 { get; set; }
        public int Base26 { get; set; }
        public int Base27 { get; set; }
        public int Base28 { get; set; }
        public int Base29 { get; set; }
        public int Base30 { get; set; }

        public int Base31 { get; set; }
        public int Base32 { get; set; }
        public int Base33 { get; set; }
        public int Base34 { get; set; }
        public int Base35 { get; set; }
        public int Base36 { get; set; }
        public int Base37 { get; set; }
        public int Base38 { get; set; }
        public int Base39 { get; set; }
        public int Base40 { get; set; }

        public int Base41 { get; set; }
        public int Base42 { get; set; }
        public int Base43 { get; set; }
        public int Base44 { get; set; }
        public int Base45 { get; set; }
        public int Base46 { get; set; }
        public int Base47 { get; set; }
        public int Base48 { get; set; }
        public int Base49 { get; set; }
        public int Base50 { get; set; }

        public int Weight { get; set; }

        public void SetBase(int n, int weight)
        {
            switch(n)
            {
                case 1:
                    Base01 = weight;
                    break;
                case 2:
                    Base02 = weight;
                    break;
                case 3:
                    Base03 = weight;
                    break;
                case 4:
                    Base04 = weight;
                    break;
                case 5:
                    Base05 = weight;
                    break;
                case 6:
                    Base06 = weight;
                    break;
                case 7:
                    Base07 = weight;
                    break;
                case 8:
                    Base08 = weight;
                    break;
                case 9:
                    Base09 = weight;
                    break;
                case 10:
                    Base10 = weight;
                    break;
                case 11:
                    Base11 = weight;
                    break;
                case 12:
                    Base12 = weight;
                    break;
                case 13:
                    Base13 = weight;
                    break;
                case 14:
                    Base14 = weight;
                    break;
                case 15:
                    Base15 = weight;
                    break;
                case 16:
                    Base16 = weight;
                    break;
                case 17:
                    Base17 = weight;
                    break;
                case 18:
                    Base18 = weight;
                    break;
                case 19:
                    Base19 = weight;
                    break;
                case 20:
                    Base20 = weight;
                    break;
                case 21:
                    Base21 = weight;
                    break;
                case 22:
                    Base22 = weight;
                    break;
                case 23:
                    Base23 = weight;
                    break;
                case 24:
                    Base24 = weight;
                    break;
                case 25:
                    Base25 = weight;
                    break;
                case 26:
                    Base26 = weight;
                    break;
                case 27:
                    Base27 = weight;
                    break;
                case 28:
                    Base28 = weight;
                    break;
                case 29:
                    Base29 = weight;
                    break;
                case 30:
                    Base30 = weight;
                    break;
                case 31:
                    Base31 = weight;
                    break;
                case 32:
                    Base32 = weight;
                    break;
                case 33:
                    Base33 = weight;
                    break;
                case 34:
                    Base34 = weight;
                    break;
                case 35:
                    Base35 = weight;
                    break;
                case 36:
                    Base36 = weight;
                    break;
                case 37:
                    Base37 = weight;
                    break;
                case 38:
                    Base38 = weight;
                    break;
                case 39:
                    Base39 = weight;
                    break;
                case 40:
                    Base40 = weight;
                    break;
                case 41:
                    Base41 = weight;
                    break;
                case 42:
                    Base42 = weight;
                    break;
                case 43:
                    Base43 = weight;
                    break;
                case 44:
                    Base44 = weight;
                    break;
                case 45:
                    Base45 = weight;
                    break;
                case 46:
                    Base46 = weight;
                    break;
                case 47:
                    Base47 = weight;
                    break;
                case 48:
                    Base48 = weight;
                    break;
                case 49:
                    Base49 = weight;
                    break;
                case 50:
                    Base50 = weight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("n");
            }
        }

        public int GetBase(int n)
        {
            switch (n)
            {
                case 1:
                    return Base01;
                case 2:
                    return Base02;
                case 3:
                    return Base03;
                case 4:
                    return Base04;
                case 5:
                    return Base05;
                case 6:
                    return Base06;
                case 7:
                    return Base07;
                case 8:
                    return Base08;
                case 9:
                    return Base09;
                case 10:
                    return Base10;
                case 11:
                    return Base11;
                case 12:
                    return Base12;
                case 13:
                    return Base13;
                case 14:
                    return Base14;
                case 15:
                    return Base15;
                case 16:
                    return Base16;
                case 17:
                    return Base17;
                case 18:
                    return Base18;
                case 19:
                    return Base19;
                case 20:
                    return Base20;
                case 21:
                    return Base21;
                case 22:
                    return Base22;
                case 23:
                    return Base23;
                case 24:
                    return Base24;
                case 25:
                    return Base25;
                case 26:
                    return Base26;
                case 27:
                    return Base27;
                case 28:
                    return Base28;
                case 29:
                    return Base29;
                case 30:
                    return Base30;
                case 31:
                    return Base31;
                case 32:
                    return Base32;
                case 33:
                    return Base33;
                case 34:
                    return Base34;
                case 35:
                    return Base35;
                case 36:
                    return Base36;
                case 37:
                    return Base37;
                case 38:
                    return Base38;
                case 39:
                    return Base39;
                case 40:
                    return Base40;
                case 41:
                    return Base41;
                case 42:
                    return Base42;
                case 43:
                    return Base43;
                case 44:
                    return Base44;
                case 45:
                    return Base45;
                case 46:
                    return Base46;
                case 47:
                    return Base47;
                case 48:
                    return Base48;
                case 49:
                    return Base49;
                case 50:
                    return Base50;
                default:
                    throw new ArgumentOutOfRangeException("n");
            }
        }

    }
}
