using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MyFundex.Api.Infrastructure;

public static class InputValidation
{
    public static Dictionary<string, string[]> Registration(
        string? email,
        string? password,
        string? firstName,
        string? lastName
    )
    {
        var errors = new Dictionary<string, string[]>();
        if (
            string.IsNullOrWhiteSpace(email)
            || email.Length > 254
            || !new EmailAddressAttribute().IsValid(email)
        )
            errors["email"] = ["Enter a valid email address."];
        if (
            string.IsNullOrWhiteSpace(password)
            || password.Length < 12
            || Encoding.UTF8.GetByteCount(password) > 72
        )
            errors["password"] = ["Use at least 12 characters and no more than 72 UTF-8 bytes."];
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100)
            errors["firstName"] = ["First name is required (maximum 100 characters)."];
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
            errors["lastName"] = ["Last name is required (maximum 100 characters)."];
        return errors;
    }
}
