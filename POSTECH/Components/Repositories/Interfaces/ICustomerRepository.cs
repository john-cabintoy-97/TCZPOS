using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<CustomerModels>> GetAllCustomersAsync();
        Task<List<CustomerModels>> SearchCustomersAsync(string query);
        Task<bool> AddCustomerAsync(CustomerModels customer);
        Task<CustomerModels?> GetCustomerByIdAsync(int id);
        Task<bool> UpdateCustomerAsync(CustomerModels customer);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
