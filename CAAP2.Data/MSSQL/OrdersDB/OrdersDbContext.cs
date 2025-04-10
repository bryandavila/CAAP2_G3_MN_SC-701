using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CAAP2.Data.MSSQL.OrdersDB;

public partial class OrdersDbContext : DbContext
{
    public OrdersDbContext()
    {
    }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderType> OrderTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=BADM-DAVILA;Database=OrdersDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderID).HasName("PK__Order__C3905BAFDB167BB6");

            entity.ToTable("Order");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderDetail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OrderTypeId).HasDefaultValue(1);
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.OrderType).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_OrderType");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserID__412EB0B6");
        });

        modelBuilder.Entity<OrderType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderTyp__3214EC07B453428C");

            entity.ToTable("OrderType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("PK__User__1788CCAC85FFC105");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053467708B69").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsPremium).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
