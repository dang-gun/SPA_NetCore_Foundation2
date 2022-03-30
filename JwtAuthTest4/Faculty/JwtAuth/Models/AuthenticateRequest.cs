namespace WebApi.Models.Accounts;

using System.ComponentModel.DataAnnotations;

public class AuthenticateRequest
{
    [Required]
    public string SignName { get; set; }

    [Required]
    public string Password { get; set; }
}