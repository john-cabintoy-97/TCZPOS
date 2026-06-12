using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class CustomerService(ICustomerRepository _repo)
    {
        public List<CustomerModels> CustomerList { get; private set; } = new();

        public async Task<List<CustomerModels>> GetAllCustomers()
        {
            CustomerList = await _repo.GetAllCustomersAsync();
            return CustomerList;
        }

        public async Task<List<CustomerModels>> SearchCustomers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return CustomerList;
            return await _repo.SearchCustomersAsync(query);
        }

        public async Task<bool> AddCustomer(CustomerModels customer)
        {
            bool isSuccess = await _repo.AddCustomerAsync(customer);

            if (isSuccess)
            {
                await GetAllCustomers(); 
                return true;
            }
            return false;
        }

        public async Task<CustomerModels?> GetCustomerByIdAsync(int id)
            => await _repo.GetCustomerByIdAsync(id);

        public async Task<bool> UpdateCustomer(CustomerModels customer)
        {
            bool isSuccess = await _repo.UpdateCustomerAsync(customer);

            if (isSuccess)
            {
                await GetAllCustomers();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCustomer(int id)
        {
            bool isSuccess = await _repo.DeleteCustomerAsync(id);

            if (isSuccess)
            {
                await GetAllCustomers();
                return true;
            }
            return false;
        }
    }


}
