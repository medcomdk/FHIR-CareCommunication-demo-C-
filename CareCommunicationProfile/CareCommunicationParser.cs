using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using static Hl7.Fhir.Model.MessageHeader;

namespace CareCommunicationProfile;

public static class CareCommunicationParser
{
    public static CareCommunicationMessageDTO FromJson(string json) =>
        new CareCommunicationMessageMapper().Map(new FhirJsonParser().Parse<Bundle>(json));

    public static CareCommunicationMessageDTO FromXml(string xml) =>
        new CareCommunicationMessageMapper().Map(new FhirXmlParser().Parse<Bundle>(xml));
}
