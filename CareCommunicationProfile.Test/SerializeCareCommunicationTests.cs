using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using Xunit;

namespace CareCommunicationProfile.Test;

public class Validator
{
    private readonly ValidationSettings settings;

    public Validator()
    {
        var zipFile = @"..\..\..\Definitions\definitions.xml.zip";

        var resolvers = new List<IAsyncResourceResolver>();
        resolvers.Add(new CachedResolver(ZipSource.CreateValidationSource()));
        resolvers.Add(new CachedResolver(new ZipSource(zipFile)));

        settings = ValidationSettings.CreateDefault();
        var multiResolver = new MultiResolver(resolvers);

        settings.ResourceResolver = multiResolver;
        settings.GenerateSnapshot = true;
    }

    internal OperationOutcome Validate(Bundle? message)
    {
        var validator = new Hl7.Fhir.Validation.Validator(settings);
        return validator.Validate(message);
    }

}

public class SerializeCareCommunicationTests : IClassFixture<Validator>
{
    private readonly Validator validator;

    public SerializeCareCommunicationTests(Validator validator)
    {
        this.validator = validator;
    }

    private CareCommunicationMessageDTO GetTestMessage()
    {
        var sender = new MessagingOrganizationDTO("Sender Organization", "123456789012345", "5790001382445");
        var primaryReceiver = new MessagingOrganizationDTO("Receiver Organization", "543210987654321", "5790000121526");

        var subject = new PatientDTO("Eric Flame", new List<AddressDTO>(), false);
        var category = "carecoordination";
        var topic = (string?)null;

        var timestamp = DateTimeOffset.Now;
        var author = "Michael Burns";
        var text = "The burns are quite severe";
        var payloadTexts = new List<PayloadTextDTO>() { new PayloadTextDTO(timestamp, author, text) };

        var careCommunication = new CareCommunicationDTO(subject, category, topic, payloadTexts);

        return new CareCommunicationMessageDTO(sender, primaryReceiver, careCommunication);
    }

    [Fact]
    public void SerializeMessageAsXml()
    {
        var messageDTO = GetTestMessage();

        var xml = new CareCommunicationSerializer().SerializeAsXML(messageDTO);

        var message = new FhirXmlParser().Parse<Bundle>(xml);
        var outcome = validator.Validate(message);
        Assert.Equal(0, outcome.Errors);
    }

    [Fact]
    public void SerializeMessageAsJson()
    {
        var messageDTO = GetTestMessage();

        var json = new CareCommunicationSerializer().SerializeAsJson(messageDTO);

        var message = new FhirJsonParser().Parse<Bundle>(json);
        var outcome = validator.Validate(message);
        Assert.Equal(0, outcome.Errors);
    }
}
