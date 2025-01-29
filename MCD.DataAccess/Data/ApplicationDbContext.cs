using Microsoft.EntityFrameworkCore;
using MCD.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MCD.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)//just configeration
        { }
        // after creating the table content in the model create a dbset object here in order to add it in the db
        // after that add a new migration to create a table with the name of (the variable name
        // then update the database (Package Manager Console)
        public DbSet<Document> Documents { get; set; }

        // after Appling ocr we will have a table of the extracted files contents
        public DbSet<ExtractedDocument> ExtractedDocuments { get; set; }

        // after summarizing the text file reference will be saved in the SummarizedDocument table
        public DbSet<SummarizedDocument> SummarizedDocuments { get; set; }

        // create also category table
        public DbSet<Category> Categories { get; set; }

        // create entities table
        public DbSet<Entity> Entities { get; set; }

        //ai module types table -> ocr, nlp, etc...
        public DbSet<AIModule> AIModules { get; set; }

        // to access a document of another user -> authorized users
        public DbSet<SharedDocument> SharedDocuments { get; set; }

        //audit logs table -> for any action in a document
        public DbSet<AuditLog> AuditLogs { get; set; }

        //here to create the user table
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }







        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //to seed tables:

            //modelBuilder.Entity<Document>().HasData(
            //    new Document { }
            //    );
        }


    }
}
