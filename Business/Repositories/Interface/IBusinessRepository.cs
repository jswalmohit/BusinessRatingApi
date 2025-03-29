using Business.Dto;
using Business.Models;

namespace Business.Repositories.Interface
{
    public interface IBusinessRepository
    {
        public  Task<bool> RegisterBusiness(BusinesDto businesDto);
        public Task<List<Busines>> SearchBusiness(int businessId);
        public Task<Busines> SearchBusinessById(int businessId);
    }
}
