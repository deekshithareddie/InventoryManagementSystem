using IMSWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IMSWebApi.Data
{
    public class InventoryDBaseContext : DbContext
    {
        public InventoryDBaseContext()
        {
        }

        public InventoryDBaseContext(DbContextOptions<InventoryDBaseContext> options) : base(options)
        {
        }


        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Supplier> Suppliers { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer("Data Source=.\\MSSQLSERVER01;Initial Catalog=InventoryDBase;Integrated Security=True;TrustServerCertificate=True");

    }
}