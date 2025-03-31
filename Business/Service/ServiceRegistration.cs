using Business.Repositories;
using Business.Repositories.Interface;

namespace Business.Service
{
    public static class ServiceRegistration
    {
        public static void  AddCustomServices(this IServiceCollection services)
        {
            services.AddTransient<EmailService>();
            services.AddTransient<SubAdminServices>();
            services.AddTransient<IBusinessRepository, BusinessRepository>();
        }
    }
}
