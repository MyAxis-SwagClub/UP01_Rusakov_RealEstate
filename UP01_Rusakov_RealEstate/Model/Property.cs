using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Rusakov_RealEstate.Model
{
    [Table("Property")]
    public class Property
    {
        public int ID { get; set; }
        public int TypeID { get; set; }
        public int UserID { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public decimal Area { get; set; }
        public int? Rooms { get; set; }
        public decimal Price { get; set; }
        public string Photo { get; set; }

        [ForeignKey("TypeID")]
        public virtual PropertyType PropertyType { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
