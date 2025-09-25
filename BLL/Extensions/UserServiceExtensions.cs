using BLL.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions
{
    public static class UserServiceExtensions
    {
        /// <summary>
        /// Роль устанавливаемая пользователю по умолчанию
        /// </summary>
        public static IServiceCollection AddDefaultUserRole(this IServiceCollection services, RoleType roleName)
        {
            DefaultRoleConfig.DefaultRoleName = roleName.ToString();

            return services;
        }
    }
}