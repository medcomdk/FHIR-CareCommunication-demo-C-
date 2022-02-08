using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace CareCommunicationProfile.Test;

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

        var careCommunication = new CareCommunicationDTO(timestamp, subject, category, topic, payloadTexts);

        return new CareCommunicationMessageDTO(timestamp, sender, primaryReceiver, careCommunication);
    }

    [Fact]
    public void SerializeMessageAsXml()
    {
        var messageDTO = GetTestMessage();

        var xml = CareCommunicationSerializer.SerializeAsXML(messageDTO);

        var message = new FhirXmlParser().Parse<Bundle>(xml);
        var outcome = validator.Validate(message);
        Assert.Equal(4, outcome.Errors); // Expected errors due to missing fhirpath operations in the official validator
    }

    [Fact]
    public void SerializeMessageAsJson()
    {
        var messageDTO = GetTestMessage();

        var json = CareCommunicationSerializer.SerializeAsJson(messageDTO);

        var message = new FhirJsonParser().Parse<Bundle>(json);
        var outcome = validator.Validate(message);
        Assert.Equal(4, outcome.Errors); // Expected errors due to missing fhirpath operations in the official validator
    }
}
