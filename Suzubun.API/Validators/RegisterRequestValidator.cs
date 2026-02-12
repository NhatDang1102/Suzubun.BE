using FluentValidation;
using Suzubun.API.Controllers;

namespace Suzubun.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MinimumLength(2).WithMessage("Tên quá ngắn.")
            .MaximumLength(50).WithMessage("Tên quá dài.")
            .Must(NotBeSpam).WithMessage("Tên không hợp lệ hoặc có dấu hiệu spam.");
    }

    private bool NotBeSpam(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        
        // Kiểm tra đơn giản: không cho phép 1 ký tự lặp lại quá 3 lần liên tiếp (vd: aaaa)
        for (int i = 0; i < name.Length - 3; i++)
        {
            if (name[i] == name[i+1] && name[i] == name[i+2] && name[i] == name[i+3])
                return false;
        }
        
        return true;
    }
}
