using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Rusakov_RealEstate.Model
{
    [Table("User")]
    public class User
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FIO { get; set; }
        public string Photo { get; set; }

        public virtual ICollection<Property> Properties { get; set; }
    }
}
