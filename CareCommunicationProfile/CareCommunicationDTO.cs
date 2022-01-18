using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Communication;

namespace CareCommunicationProfile;

public record CareCommunicationDTO(
    PatientDTO Subject,
    string Category,
    string? Topic,
    List<PayloadTextDTO> PayloadTexts);


internal class CareCommunicationMapper : FhirMapper
{
    private readonly string CategoryCodes = $"{BaseUri}CodeSystem/medcom-careCommunication-categoryCodes";

    public CareCommunicationDTO Map(Communication fhirCommunication, Func<string?, Resource?> resourceLocator)
    {
        var subject = MapSubject(fhirCommunication.Subject, resourceLocator);
        string category = MapCategory(fhirCommunication.Category);
        var topic = fhirCommunication.Topic?.Text;
        var payloadTexts = MapPayloadTexts(fhirCommunication.Payload, resourceLocator).ToList();

        return new CareCommunicationDTO(subject, category, topic, payloadTexts);
    }
    public Communication Map(CareCommunicationDTO careCommunication, Action<Resource, string> resourceAppender)
    {
        var fhirCommunication = new Communication();
        fhirCommunication.Id = Guid.NewGuid().ToString();
        fhirCommunication.Status = EventStatus.Unknown;

        fhirCommunication.Subject = MapSubject(careCommunication.Subject, resourceAppender);
        fhirCommunication.Category = MapCategory(careCommunication.Category);
        fhirCommunication.Topic = new CodeableConcept() { Text = careCommunication.Topic };
        fhirCommunication.Payload = MapPayloadTexts(careCommunication.PayloadTexts, resourceAppender).ToList();

        return fhirCommunication;
    }

    private static ResourceReference MapSubject(PatientDTO subject, Action<Resource, string> resourceAppender)
    {
        var fhirPatient = new PatientMapper().Map(subject);
        var fhirPatientUrl = $"urn:uuid:{fhirPatient.Id}";

        resourceAppender(fhirPatient, fhirPatientUrl);

        return new ResourceReference(fhirPatientUrl);
    }

    private static PatientDTO MapSubject(ResourceReference fhirSubjectReference, Func<string?, Resource?> resourceLocator)
    {
        if (resourceLocator(fhirSubjectReference.Reference) is not Patient fhirPatient)
            throw new InvalidOperationException("Patient resource is not found");

        var subject = new PatientMapper().Map(fhirPatient);
        return subject;
    }

    private static string MapCategory(List<CodeableConcept> fhirCategories) =>
        (fhirCategories.FirstOrDefault()?.Coding.FirstOrDefault()?.Code) ?? throw new InvalidOperationException("Category value is not found");

    private List<CodeableConcept> MapCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new InvalidOperationException("Category value is empty");

        return new List<CodeableConcept>() { new CodeableConcept(CategoryCodes, category) };
    }

    private static IEnumerable<PayloadTextDTO> MapPayloadTexts(List<PayloadComponent> payloads, Func<string?, Resource?> resourceLocator)
    {
        var payloadTextMapper = new PayloadTextMapper();
        var textPayloadComponents = payloads.Where(p => p.Content.TypeName == "string");

        foreach (var textPayloadComponent in textPayloadComponents)
            yield return payloadTextMapper.Map(textPayloadComponent, resourceLocator);
    }
    private IEnumerable<PayloadComponent> MapPayloadTexts(List<PayloadTextDTO> payloadTexts, Action<Resource, string> resourceAppender)
    {
        var payloadTextMapper = new PayloadTextMapper();

        foreach (var payloadText in payloadTexts)
            yield return payloadTextMapper.Map(payloadText, resourceAppender);
    }
}
