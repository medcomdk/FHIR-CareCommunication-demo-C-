using Hl7.Fhir.Model;

namespace CareCommunicationProfile;

public record AddressDTO(string Line, string PostalCode, string City);

internal class AddressMapper
{
    public List<Address> Map(List<AddressDTO> addresses)
    {
        var result = new List<Address>();

        foreach (var address in addresses)
        {
            var fhirAddress = new Address();

            fhirAddress.Line = new List<string>() { address.Line };
            fhirAddress.PostalCode = address.PostalCode;
            fhirAddress.City = address.City;

            result.Add(fhirAddress);
        }

        return result;
    }
    public List<AddressDTO> Map(List<Address> fhirAddresses)
    {
        var result = new List<AddressDTO>();

        foreach(var fhirAddress in fhirAddresses)
            result.Add(new AddressDTO(string.Join(' ', fhirAddress.Line), fhirAddress.PostalCode, fhirAddress.City));

        return result;
    }
}