using BLL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Создание роли по умолчанию
        /// </summary>
        /// <param name="roles">Например: "User","Administrator"</param>
        public static IServiceCollection AddIdentityRoles(this IServiceCollection services, params RoleType[] roles)
        {
            if (roles.Length == 0)
                throw new ArgumentNullException("Не указана роль для создания");

            // Временно создаем сервис-провайдер
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (var role in roles)
                {
                    var roleName = role.ToString();
                    var roleExists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
                    if (!roleExists)
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                    }
                }
            }

            return services;
        }
    }
}