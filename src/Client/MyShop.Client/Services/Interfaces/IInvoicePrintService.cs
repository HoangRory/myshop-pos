using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IInvoicePrintService
    {
        void ExportToXps(Order order, string filePath);
    }
}