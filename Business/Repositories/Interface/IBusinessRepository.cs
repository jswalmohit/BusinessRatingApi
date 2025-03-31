using Business.Dto;
using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace Business.Repositories.Interface
{
    public interface IBusinessRepository
    {
        public Task<bool> RegisterBusiness(BusinesDto businesDto);
        public Task<IEnumerable<Object>> SearchBusiness(string category, string subcategory);
        public Task<Busines> SearchBusinessById(int businessId);
        public Task<bool> UpdateBusiness(BusinesDto businesDto);
        public Task<bool> UpdateBusinessDetails(BusinesDto businesDto);
        public Task<bool> CheckEmailExistsBusiness(string email);
        public Task<IEnumerable<Object>> GetCategories();
        public Task<IEnumerable<Object>> GetSubCategories(int categoryId);
        public Task<IEnumerable<Object>> GetBusineesDetailById(string category, string subcategory);

    }
}
