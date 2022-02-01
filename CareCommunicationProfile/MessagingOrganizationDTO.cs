using Hl7.Fhir.Model;

namespace CareCommunicationProfile;

public record MessagingOrganizationDTO(string Name, string SOR, string EAN);

internal class MessagingOrganizationMapper : FhirMapper
{
    private const string SOR = "urn:oid:1.2.208.176.1.1";
    private const string EAN = "urn:oid:1.3.88";

    public MessagingOrganizationDTO Map(Organization fhirOrganization)
    {
        var name = fhirOrganization.Name;

        var sor = fhirOrganization.Identifier.FirstOrDefault(i => i.System == SOR)?.Value;
        if (sor == null)
            throw new InvalidOperationException("SOR code not found for organization");

        var ean = fhirOrganization.Identifier.FirstOrDefault(i => i.System == EAN)?.Value;
        if (ean == null)
            throw new InvalidOperationException("EAN code not found for organization");

        return new MessagingOrganizationDTO(name, sor, ean);
    }

    public Organization Map(MessagingOrganizationDTO primaryReceiver)
    {
        var fhirOrganization = new Organization();

        fhirOrganization.Id = Guid.NewGuid().ToString();
        fhirOrganization.Name = primaryReceiver.Name;
        fhirOrganization.Identifier.Add(new Identifier(SOR, primaryReceiver.SOR));
        fhirOrganization.Identifier.Add(new Identifier(EAN, primaryReceiver.EAN));

        return fhirOrganization;
    }
}

