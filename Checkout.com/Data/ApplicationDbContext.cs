using Checkout.com.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System.Text;

namespace Checkout.com.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IEncryptionProvider _provider;
        private readonly byte[] _key = Encoding.ASCII.GetBytes("U7dl9pBzWEReeYkC");

        public virtual DbSet<Payment> Payments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _provider = new AesProvider(_key);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_provider);
        }
    }
}
