using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ProfileService.Core.Domain.Entities;

[Owned]
public class Address
{
    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    [RegularExpression("[0-9]{3,}", ErrorMessage = "Postal Code must contain only digits.")]
    public string PostalCode { get; set; }

    public string Country { get; set; }

    public override string? ToString()
    {
        return new Dictionary<string, string>
        {
            { "Street", Street },
            { "City", City },
            { "State", State },
            { "PostalCode", PostalCode },
            { "Country", Country }
        }.ToString();
    }

    public static Address operator +(Address addr1, Address addr2)
    {
        var address = (Address)addr1.MemberwiseClone();
        address.PostalCode = addr1.PostalCode + addr2.PostalCode;

        return address;
    }
}