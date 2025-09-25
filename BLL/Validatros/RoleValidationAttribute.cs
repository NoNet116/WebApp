using System.ComponentModel.DataAnnotations;

namespace BLL.Validatros
{
    public class RoleValidationAttribute : ValidationAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleValidationAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            var role = value.ToString();
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return _allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Поле {name} содержит недопустимую роль. Допустимые значения: {string.Join(", ", _allowedRoles)}.";
        }
    }
}