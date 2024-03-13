using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    //[Required]
    public string Token { get; set; }

    //[Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    //[Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; }
}
