using TCZPOS.Components.Models;
using SQLite;
using System.Reflection;

namespace TCZPOS.Components.ServiceRegistration
{
    public class ServiceTableRegistration
    {
        public static async Task CreateTablesAsync(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync<UserModels>();
            await connection.CreateTableAsync<ProductModels>();
            await connection.CreateTableAsync<CategoryModels>();
            await connection.CreateTableAsync<BrandModels>();
            await connection.CreateTableAsync<HeldTransactionModel>();
            await connection.CreateTableAsync<CartModels>();
            await connection.CreateTableAsync<SaleDetailModels>();
            await connection.CreateTableAsync<SaleModels>();
            await connection.CreateTableAsync<StockInModels>();
            await connection.CreateTableAsync<VatModels>();
            await connection.CreateTableAsync<VendorModels>();
            await connection.CreateTableAsync<HeldTransactionModel>();
            await connection.CreateTableAsync<AIProductModels>();
            await connection.CreateTableAsync<ProductLearningHistoryModels>();
            await connection.CreateTableAsync<StaffModels>();
            await connection.CreateTableAsync<CustomerModels>();
            await connection.CreateTableAsync<CustomerCreditModels>();
            await connection.CreateTableAsync<PurchaseOrderItemModels>();
            await connection.CreateTableAsync<PurchaseOrderModels>();
            await connection.CreateTableAsync<PrinterConfigModels>();

        }
    }
}
