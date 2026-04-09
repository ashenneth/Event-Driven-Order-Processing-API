using System.Net.Mail;
using OrderApi.DTOs;
using OrderApi.Services.Exceptions;

namespace OrderApi.Services.Validation;

public static class OrderRequestValidator
{
    public static void Validate(CreateOrderRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            Add(errors, nameof(request.CustomerEmail), "CustomerEmail is required.");
        else if (!IsValidEmail(request.CustomerEmail))
            Add(errors, nameof(request.CustomerEmail), "CustomerEmail is not a valid email address.");

        if (request.Items is null || request.Items.Count == 0)
            Add(errors, nameof(request.Items), "At least 1 order item is required.");
        else
        {
            for (var i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var prefix = $"Items[{i}]";

                if (string.IsNullOrWhiteSpace(item.Sku))
                    Add(errors, $"{prefix}.Sku", "Sku is required.");

                if (item.Quantity < 1)
                    Add(errors, $"{prefix}.Quantity", "Quantity must be >= 1.");

                if (item.UnitPrice < 0)
                    Add(errors, $"{prefix}.UnitPrice", "UnitPrice must be >= 0.");
            }

            var sum = request.Items.Sum(x => x.Quantity * x.UnitPrice);
            if (sum != request.TotalAmount)
                Add(errors, nameof(request.TotalAmount), $"TotalAmount must equal sum(items). Expected {sum:0.00}.");
        }

        if (errors.Count > 0)
        {
            var normalized = errors.ToDictionary(k => k.Key, v => v.Value.ToArray());
            throw new ValidationException("Validation failed.", normalized);
        }
    }

    private static void Add(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
        {
            list = new List<string>();
            errors[key] = list;
        }
        list.Add(message);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
