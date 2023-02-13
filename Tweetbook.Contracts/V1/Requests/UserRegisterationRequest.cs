using System.ComponentModel.DataAnnotations;

namespace Tweetbook.Contracts.V1.Requests;
public class UserRegisterationRequest
{
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
}
