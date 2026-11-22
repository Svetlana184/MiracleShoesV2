using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MiracleShoesDesktop.Model;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<AvailableSize> AvailableSizes { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvailableSize>(entity =>
        {
            entity.HasKey(e => e.IdAvailableSize).HasName("available_sizes_pkey");

            entity.ToTable("available_sizes", "miracle_shoes");

            entity.HasIndex(e => new { e.IdProduct, e.IdSize }, "available_sizes_unique_position").IsUnique();

            entity.HasIndex(e => e.IdProduct, "ix_available_sizes_product");

            entity.HasIndex(e => e.IdSize, "ix_available_sizes_size");

            entity.Property(e => e.IdAvailableSize).HasColumnName("id_available_size");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdSize).HasColumnName("id_size");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("is_available");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.AvailableSizes)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("available_sizes_id_product_fkey");

            entity.HasOne(d => d.IdSizeNavigation).WithMany(p => p.AvailableSizes)
                .HasForeignKey(d => d.IdSize)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("available_sizes_id_size_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.IdCompany).HasName("companies_pkey");

            entity.ToTable("companies", "miracle_shoes");

            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.NameCompany)
                .HasMaxLength(100)
                .HasColumnName("name_company");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("orders_pkey");

            entity.ToTable("orders", "miracle_shoes");

            entity.HasIndex(e => e.DateOrder, "ix_orders_date");

            entity.HasIndex(e => e.IdUser, "ix_orders_user");

            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.DateOrder)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("date_order");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("total_cost");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("orders_id_user_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.IdOrderItem).HasName("order_items_pkey");

            entity.ToTable("order_items", "miracle_shoes");

            entity.HasIndex(e => e.IdOrder, "ix_order_items_order");

            entity.HasIndex(e => e.IdProduct, "ix_order_items_product");

            entity.HasIndex(e => e.IdSize, "ix_order_items_size");

            entity.Property(e => e.IdOrderItem).HasColumnName("id_order_item");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Cost)
                .HasPrecision(12, 2)
                .HasComputedColumnSql("((amount)::numeric * cost_for_item)", true)
                .HasColumnName("cost");
            entity.Property(e => e.CostForItem)
                .HasColumnType("money")
                .HasColumnName("cost_for_item");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdSize).HasColumnName("id_size");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("order_items_id_order_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_items_id_product_fkey");

            entity.HasOne(d => d.IdSizeNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdSize)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("order_items_id_size_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("products_pkey");

            entity.ToTable("products", "miracle_shoes");

            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.Composition).HasColumnName("composition");
            entity.Property(e => e.Cost)
                .HasColumnType("money")
                .HasColumnName("cost");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.NameProduct)
                .HasMaxLength(300)
                .HasColumnName("name_product");
            entity.Property(e => e.Subcategory)
                .HasMaxLength(100)
                .HasColumnName("subcategory");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCompany)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("products_id_company_fkey");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.IdSize).HasName("sizes_pkey");

            entity.ToTable("sizes", "miracle_shoes");

            entity.HasIndex(e => e.ShoeSize, "sizes_shoe_size_key").IsUnique();

            entity.Property(e => e.IdSize).HasColumnName("id_size");
            entity.Property(e => e.ShoeSize).HasColumnName("shoe_size");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users", "miracle_shoes");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IdUserRole).HasColumnName("id_user_role");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.SecondName)
                .HasMaxLength(100)
                .HasColumnName("second_name");
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .HasColumnName("surname");

            entity.HasOne(d => d.IdUserRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdUserRole)
                .HasConstraintName("users_id_user_role_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.IdUserRole).HasName("user_roles_pkey");

            entity.ToTable("user_roles", "miracle_shoes");

            entity.HasIndex(e => e.NameUserRole, "user_roles_name_user_role_key").IsUnique();

            entity.Property(e => e.IdUserRole).HasColumnName("id_user_role");
            entity.Property(e => e.NameUserRole)
                .HasMaxLength(70)
                .HasColumnName("name_user_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
