using Hl7.Fhir.Model;

namespace CareCommunicationProfile;

public record PatientDTO(string Name, List<AddressDTO> Address, bool Deceased);

internal class PatientMapper : FhirMapper
{
    public PatientDTO Map(Patient patient)
    {
        var name = ConvertHumanNameToText(patient.Name);
        var address = new AddressMapper().Map(patient.Address);
        var deceased = MapDeceased(patient.Deceased);

        return new PatientDTO(name, address, deceased);
    }
    public Patient Map(PatientDTO patient)
    {
        var fhirPatient = new Patient();
        fhirPatient.Id = Guid.NewGuid().ToString();

        fhirPatient.Name = ConvertTextToHumanName(patient.Name);
        fhirPatient.Address = new AddressMapper().Map(patient.Address);
        fhirPatient.Deceased = new FhirBoolean(patient.Deceased);

        return fhirPatient;
    }

    private bool MapDeceased(DataType deceased)
    {
        if (deceased is FhirBoolean result)
            return result.Value ?? false;

        if (deceased is FhirDateTime deceasedDate)
            return !string.IsNullOrWhiteSpace(deceasedDate.Value);

        return false;
    }
}
