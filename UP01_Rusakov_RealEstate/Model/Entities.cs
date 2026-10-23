using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace UP01_Rusakov_RealEstate.Model
{
    public class Entities : DbContext
    {
        private static Entities _context;

        public Entities() : base("name=Entities")
        {
            Database.SetInitializer<Entities>(null);
        }

        public static Entities GetContext()
        {
            if (_context == null)
                _context = new Entities();
            return _context;
        }

        public static void Refresh()
        {
            List<DbEntityEntry> entries = new List<DbEntityEntry>(GetContext().ChangeTracker.Entries());
            foreach (DbEntityEntry entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.State = EntityState.Detached;
                else
                    entry.Reload();
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<Property> Properties { get; set; }
    }
}
