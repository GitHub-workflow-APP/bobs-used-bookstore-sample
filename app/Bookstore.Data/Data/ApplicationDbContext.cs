﻿using System;
using System.IO;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using BookType = Bookstore.Domain.Books.BookType;

namespace Bookstore.Data.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Address { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<Condition> Condition { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Genre> Genre { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Price> Price { get; set; }
        public DbSet<Publisher> Publisher { get; set; }
        public DbSet<BookType> Type { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Resale> Resale { get; set; }
        public DbSet<ResaleStatus> ResaleStatus { get; set; }
        public DbSet<ReferenceDataItem> ReferenceData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateSeedResaleStatus(modelBuilder);
        }

        public void CreateSeedResaleStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResaleStatus>().HasData(new ResaleStatus
            { ResaleStatus_Id = 1, Status = "Pending Approval" });
            modelBuilder.Entity<ResaleStatus>().HasData(new ResaleStatus
            { ResaleStatus_Id = 2, Status = "Approved/Awaiting Shipment from Customer" });
            modelBuilder.Entity<ResaleStatus>().HasData(new ResaleStatus { ResaleStatus_Id = 3, Status = "Rejected" });
            modelBuilder.Entity<ResaleStatus>().HasData(new ResaleStatus
            { ResaleStatus_Id = 4, Status = "Shipment Receipt Confirmed" });
            modelBuilder.Entity<ResaleStatus>().HasData(new ResaleStatus
            { ResaleStatus_Id = 5, Status = "Payment Completed" });
        }
    }


    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environmentName =
               Environment.GetEnvironmentVariable(
                   "ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Directory.GetCurrentDirectory() + "/../frontend/appsettings.json")
                .AddJsonFile(Directory.GetCurrentDirectory() + $"/../frontend/appsettings.{environmentName}.json", true)

                .Build();
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("BobBookstoreContextConnection");
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("DataMigrations"));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
