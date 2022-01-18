using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.MessageHeader;

namespace CareCommunicationProfile;

public record CareCommunicationMessageDTO(
    string EventCode,
    MessagingOrganizationDTO Sender,
    MessagingOrganizationDTO PrimaryReceiver,
    CareCommunicationDTO CareCommunication);

internal class CareCommunicationMessageMapper : FhirMapper
{
    private readonly string DestinationUseExtension = $"{FhirMapper.BaseUri}StructureDefinition/medcom-messaging-destinationUseExtension";
    private readonly string EventCodes = $"{FhirMapper.BaseUri}CodeSystem/medcom-messaging-eventCodes";
    private readonly string System = $"{FhirMapper.BaseUri}CodeSystem/medcom-messaging-destinationUse";

    private const string UsePrimary = "primary";
    private const string UnknownEndPoint = "http://medcomfhir.dk/unknown";

    public CareCommunicationMessageDTO Map(Bundle message)
    {
        var resourceLocator = (string? reference) => message.Entry.FirstOrDefault(e => e.FullUrl == reference)?.Resource;

        if (message.Entry.FirstOrDefault()?.Resource is not MessageHeader fhirMessageHeader)
            throw new InvalidOperationException("MessageHeader resource not found");

        string eventCode = MapEventCode(fhirMessageHeader.Event as Coding);
        var sender = MapSender(fhirMessageHeader.Sender, resourceLocator);
        var primaryReceiver = MapPrimaryReceiver(fhirMessageHeader.Destination, resourceLocator);
        var communicationDTO = MapCommunication(fhirMessageHeader.Focus, resourceLocator);

        return new CareCommunicationMessageDTO(eventCode, sender, primaryReceiver, communicationDTO);
    }
    public Bundle Map(CareCommunicationMessageDTO messageDTO)
    {
        var result = new Bundle();
        result.Type = Bundle.BundleType.Message;

        Action<Resource, string> resourceAppender = (resource, url) => result.AddResourceEntry(resource, url);

        var fhirMessageHeader = new MessageHeader();
        fhirMessageHeader.Id = Guid.NewGuid().ToString();
        fhirMessageHeader.Event = new Coding(EventCodes, messageDTO.EventCode);

        resourceAppender(fhirMessageHeader, $"urn:uuid:{fhirMessageHeader.Id}");

        fhirMessageHeader.Destination = MapPrimaryReceiver(messageDTO.PrimaryReceiver, resourceAppender);
        fhirMessageHeader.Sender = MapSender(messageDTO.Sender, resourceAppender);
        fhirMessageHeader.Focus = MapCommunication(messageDTO.CareCommunication, resourceAppender);
        fhirMessageHeader.Source = new MessageSourceComponent { Endpoint = UnknownEndPoint };

        return result;
    }


    private static string MapEventCode(Coding? @event) => @event?.Code ?? throw new InvalidOperationException("Event code not found");

    private static MessagingOrganizationDTO MapSender(ResourceReference fhirSenderReference, Func<string?, Resource?> resourceLocator)
    {
        if (resourceLocator(fhirSenderReference.Reference) is not Organization fhirSender)
            throw new InvalidOperationException("Sender resource is not found");

        return new MessagingOrganizationMapper().Map(fhirSender);
    }
    private static ResourceReference MapSender(MessagingOrganizationDTO senderDTO, Action<Resource, string> resourceAppender)
    {
        var sender = new MessagingOrganizationMapper().Map(senderDTO);
        var senderUrl = $"urn:uuid:{sender.Id}";

        resourceAppender(sender, senderUrl);

        return new ResourceReference(senderUrl);
    }

    private MessagingOrganizationDTO MapPrimaryReceiver(List<MessageDestinationComponent> destination, Func<string?, Resource?> resourceLocator)
    {
        var fhirPrimaryDestination = destination.FirstOrDefault(d => d.GetExtensionValue<Coding>(DestinationUseExtension)?.Code == UsePrimary);
        var fhirPrimaryReceiver = resourceLocator(fhirPrimaryDestination?.Receiver.Reference) as Organization;
        if (fhirPrimaryReceiver == null)
            throw new InvalidOperationException("Primary receiver resource is not found");

        return new MessagingOrganizationMapper().Map(fhirPrimaryReceiver);
    }
    private List<MessageDestinationComponent> MapPrimaryReceiver(MessagingOrganizationDTO primaryReceiver, Action<Resource, string> resourceAppender)
    {
        var receiver = new MessagingOrganizationMapper().Map(primaryReceiver);
        var receiverUrl = $"urn:uuid:{receiver.Id}";
        resourceAppender(receiver, receiverUrl);

        var fhirMessageDestinationComponent = new MessageDestinationComponent();
        fhirMessageDestinationComponent.Endpoint = UnknownEndPoint;
        fhirMessageDestinationComponent.AddExtension(DestinationUseExtension, new Coding(System, UsePrimary));
        fhirMessageDestinationComponent.Receiver = new ResourceReference(receiverUrl);

        return new List<MessageDestinationComponent> { fhirMessageDestinationComponent };
    }


    private static CareCommunicationDTO MapCommunication(List<ResourceReference> fhirCommunicationReferences, Func<string?, Resource?> resourceLocator)
    {
        if (resourceLocator(fhirCommunicationReferences.FirstOrDefault()?.Reference) is not Communication fhirCommunication)
            throw new InvalidOperationException("Communication resource not found");

        return new CareCommunicationMapper().Map(fhirCommunication, resourceLocator);
    }
    private List<ResourceReference> MapCommunication(CareCommunicationDTO careCommunication, Action<Resource, string> resourceAppender)
    {
        Communication fhirCommunication = new CareCommunicationMapper().Map(careCommunication, resourceAppender);
        var fhirCommunicationUrl = $"urn:uuid:{fhirCommunication.Id}";

        resourceAppender(fhirCommunication, fhirCommunicationUrl);

        return new List<ResourceReference>() { new ResourceReference(fhirCommunicationUrl) };
    }
}