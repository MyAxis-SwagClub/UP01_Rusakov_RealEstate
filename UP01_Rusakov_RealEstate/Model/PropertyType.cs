using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Rusakov_RealEstate.Model
{
    [Table("PropertyType")]
    public class PropertyType
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Property> Properties { get; set; }
    }
}
