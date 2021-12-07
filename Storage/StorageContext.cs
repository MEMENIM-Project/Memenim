using System;
using System.IO;
using System.Reflection;
using Memenim.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Memenim.Storage
{
    public class StorageContext : DbContext
    {
        public DbSet<PostCommentDraft> PostCommentDrafts { get; set; }



        public StorageContext()
            : base()
        {
            if (!Directory.Exists(Path.GetDirectoryName(StorageManager.StorageFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(StorageManager.StorageFilePath));

            if (!File.Exists(StorageManager.StorageFilePath))
                File.Create(StorageManager.StorageFilePath).Close();
        }



        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={StorageManager.StorageFilePath}");

            base.OnConfiguring(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
