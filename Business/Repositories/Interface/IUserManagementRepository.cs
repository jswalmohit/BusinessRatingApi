namespace Business.Repositories.Interface
{
    public interface IUserManagementRepository
    {
        public List<IEnumerable<Object>> GetAllUser();
    }
}
