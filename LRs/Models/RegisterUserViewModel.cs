using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LRs.Models;

public sealed class RegisterUserViewModel
{
    [Required(ErrorMessage = "Укажите электронную почту.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "Пароли не совпадают.")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Аватар")]
    public IFormFile? Avatar { get; set; }
}