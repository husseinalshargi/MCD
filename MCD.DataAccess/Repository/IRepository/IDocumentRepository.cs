using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface IDocumentRepository : IRepository<Document> //T -> Document
    {
        void Update(Document obj);
    }
}
