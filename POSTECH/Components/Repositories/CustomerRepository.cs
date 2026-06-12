using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class CustomerRepository(DBQueries _db) : ICustomerRepository
    {
        public async Task<List<CustomerModels>> GetAllCustomersAsync()
        {
            return await _db.SelectAllAsync<CustomerModels>();
        }

        public async Task<List<CustomerModels>> SearchCustomersAsync(string query)
        {
            return await _db.SelectFilteredAsync<CustomerModels>(c =>
            c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.ContactNumber.Contains(query));
        }

        public async Task<bool> AddCustomerAsync(CustomerModels customer)
        {
            bool success = false;

            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    // 1. Insert the Customer
                    // SQLite-net automatically updates the 'customer.Id' property after this call
                    int customerRows = conn.Insert(customer);

                    if (customerRows > 0)
                    {
                        // 2. Create the linked Credit Record
                        var creditRecord = new CustomerCreditModels
                        {
                            CustomerId = customer.Id, // The link to the table above
                            CustomerName = customer.Name,
                            TotalDebt = 0,
                            Status = "Cleared",
                            LastPaymentDate = DateTime.Now
                        };

                        conn.Insert(creditRecord);
                        success = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }

            return success;
        }

        public async Task<CustomerModels?> GetCustomerByIdAsync(int id)
        {
            return await _db.GetAsync<CustomerModels>(id);
        }

        public async Task<bool> UpdateCustomerAsync(CustomerModels customer)
        {
            return await _db.UpdateAsync(customer) > 0;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            bool success = false;
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    int rowsDeleted = conn.Delete<CustomerModels>(id);
                    if (rowsDeleted > 0)
                    {
                        success = true;
                    }
                });
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }
    }
}
