using Microsoft.EntityFrameworkCore;

namespace EcorpAPI.Models
{

    public partial class ShoppingCartContext : DbContext
    {
        public ShoppingCartContext(DbContextOptions
        <ShoppingCartContext> options)
            : base(options)
        {
        }
        public virtual DbSet<ShoppingItem> ShoppingItems { get; set; }
        public virtual DbSet<UserDetails> UserDetails { get; set; }
        public virtual DbSet<ItemImage> ItemImages { get; set; }
        public virtual DbSet<CartItemModel> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingItem>(entity => {
                entity.HasKey(k => k.ItemId);
            });

            modelBuilder.Entity<UserDetails>(entity => {
                entity.HasKey(k => k.UserId);
            });

            modelBuilder.Entity<ItemImage>(entity => {
                entity.HasKey(k => k.ImageId);
            });

            modelBuilder.Entity<CartItemModel>(entity => {
                entity.HasKey(k => k.CartItemId);

                entity.HasOne(ci => ci.Item).WithMany(i => i.CartItems).HasForeignKey(ci => ci.ItemId);

            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
