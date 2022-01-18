using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Communication;

namespace CareCommunicationProfile;

public record PayloadTextDTO(DateTimeOffset Timestamp, string Author, string Text);

internal class PayloadTextMapper : FhirMapper
{
    protected readonly string DateTimeExtension = $"{BaseUri}StructureDefinition/medcom-core-datetime-extension";
    protected readonly string AuthorExtension = $"{BaseUri}StructureDefinition/medcom-core-author-extension";

    public PayloadTextDTO Map(PayloadComponent textPayloadComponent, Func<string?, Resource?> resourceLocator)
    {
        var authoredTimestamp = MapAuthoredTimestamp(textPayloadComponent);
        var authorDisplayName = MapAuthorDisplayName(textPayloadComponent, resourceLocator);
        var textContent = MapTextContent(textPayloadComponent.Content as FhirString);

        return new PayloadTextDTO(authoredTimestamp, authorDisplayName, textContent);
    }
    public PayloadComponent Map(PayloadTextDTO payloadText, Action<Resource, string> resourceAppender)
    {
        var textPayloadComponent = new PayloadComponent();

        textPayloadComponent.AddExtension(DateTimeExtension, new FhirDateTime(payloadText.Timestamp));
        textPayloadComponent.AddExtension(AuthorExtension, MapAuthorDisplayName(payloadText.Author, resourceAppender));
        textPayloadComponent.Content = new FhirString(payloadText.Text);

        return textPayloadComponent;
    }

    private DateTimeOffset MapAuthoredTimestamp(PayloadComponent textPayloadComponent)
    {
        var fhirTimestamp = textPayloadComponent.GetExtensionValue<FhirDateTime>(DateTimeExtension);
        try
        {
            if (!fhirTimestamp.TryToDateTimeOffset(out var timestamp))
                timestamp = ConvertDateOnlyAsLocalTimezone(fhirTimestamp);

            return timestamp;
        }
        catch
        {
            throw new InvalidOperationException("Illegal timestamp passed for payload component");
        }
    }

    private string MapAuthorDisplayName(PayloadComponent textPayloadComponent, Func<string?, Resource?> resourceLocator)
    {
        var fhirAuthorReference = textPayloadComponent.GetExtensionValue<ResourceReference>(AuthorExtension);
        if (resourceLocator(fhirAuthorReference.Reference) is not Practitioner fhirAuthor)
            throw new InvalidOperationException("Referenced Practitioner resource not found");

        return ConvertHumanNameToText(fhirAuthor.Name);
    }
    private ResourceReference MapAuthorDisplayName(string author, Action<Resource, string> resourceAppender)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new InvalidOperationException("Payload text author is empty");

        var fhirAuthor = new Practitioner
        {
            Id = Guid.NewGuid().ToString(),
            Name = ConvertTextToHumanName(author)
        };
        var authorUrl = $"urn:uuid:{fhirAuthor.Id}";
        resourceAppender(fhirAuthor, authorUrl);

        return new ResourceReference(authorUrl);
    }

    private static string MapTextContent(FhirString? fhirText) => fhirText?.Value.Trim() ?? string.Empty;
}
