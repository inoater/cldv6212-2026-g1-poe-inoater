using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Mappers
{
    public static class DocumentMapper  // Microsoft, 2026ac
    {
        public static StaffDocumentDto ToDto(this FunctionStaffDocument document)
        {
            return new StaffDocumentDto
            {
                FileName = document.FileName,
                SizeBytes = document.Size,
                LastModified = document.LastModified
            };
        }
    }
}
