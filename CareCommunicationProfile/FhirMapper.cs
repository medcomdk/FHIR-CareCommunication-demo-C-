using Hl7.Fhir.Model;

namespace CareCommunicationProfile;

internal class FhirMapper
{
    protected readonly static string BaseUri = "http://medcomfhir.dk/fhir/core/1.0/";

    protected string ToId(string urn) => urn.Replace("urn:uuid:", string.Empty);
    protected string ToUrn(string id) => $"urn:uuid:{id}";

    protected string ConvertHumanNameToText(List<HumanName> fhirHumanNames)
    {
        var fhirHumanName = fhirHumanNames.FirstOrDefault();
        if (fhirHumanName == null)
            throw new InvalidOperationException("HumanName not found");

        return $"{string.Join(" ", fhirHumanName.Given)} {fhirHumanName.Family}".Trim();
    }
    protected List<HumanName> ConvertTextToHumanName(string name)
    {
        var nameComponents = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var length = nameComponents.Length;
        if (length > 0)
        {
            return new List<HumanName>() {
                new HumanName
                {
                    Family = nameComponents.Last(),
                    Given = nameComponents.Take(length - 1)
                }
            };
        }
        else
            throw new InvalidOperationException("Empty human name is not allowed");
    }

    protected DateTimeOffset ConvertDateOnlyAsLocalTimezone(FhirDateTime fhirTimestamp)
    {
        DateTimeOffset timestamp;
        var localTimestamp = fhirTimestamp.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        if (localTimestamp.TimeOfDay == TimeSpan.Zero)
            timestamp = localTimestamp;
        else
            throw new InvalidOperationException();
        return timestamp;
    }
}
