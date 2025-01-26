using Microsoft.EntityFrameworkCore;
using MCD.Models;

namespace MCD.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)//just configeration
        { }
        // after creating the table content in the model create a dbset object here in order to add it in the db
        // after that add a new migration to create a table with the name of (the variable name
        // then update the database (Package Manager Console)
        public DbSet<Document> Documents { get; set; }

        // after appling ocr we will have a table of the extracted files contents
        public DbSet<ExtractedDocument> ExtractedDocuments { get; set; }


    }
}
