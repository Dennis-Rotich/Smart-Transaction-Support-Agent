using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Entities
{
    public class Document : BaseEntity
    {
        public string FileName { get; private set; }
        public string StoragePath { get; private set; }
        public string? DocumentType { get; private set; }

        public Document(string fileName, string storagePath, string? documentType = null)
        {
            FileName = fileName;
            StoragePath = storagePath;
            DocumentType = documentType;
        }
    }
}