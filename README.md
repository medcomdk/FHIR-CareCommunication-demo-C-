# Care Communication Demo in CSharp
This project demonstrates how to use the official HL7 FHIR SDK in .NET for parsing and serializing a Care Communication message according to the implementation guide provided by MedCom. It does not intend to be a full implementation but serves as a guiding sample that will make it easier to implement this and other profiles provided by MedCom. A similar sample exists for Java (https://github.com/medcomdk/FHIR-CareCommunication-demo-Java)

The implementation guide containing the Care Commmunication profile is found here: http://build.fhir.org/ig/hl7dk/kl-medcom/

Documentation of the official HL7 FHIR SDK in .NET is found here: https://fire.ly/products/firely-net-sdk/

## Prerequisites
- Your favourite IDE (Visual Studio 2022, Visual Studio Code, ...)
- .Net 6

## Projects
The solution contains four projects 
- A class library containing a partial  CareCommunicationParser and a partial CareCommunicationSerializer
- A test project containing simple tests for the parser and the serializer
- A console app to create a care communication FHIR message
- A console app to parse a care communication FHIR message

## Fhir Related NuGet Packages
- Hl7.Fhir.R4 (The Official HL7 FHIR SDK - Used in the class library)
- Hl7.Fhir.Specification.R4 (Profile Based Validation - Used in the test project)

## Parsing from Fhir
The CareCommunication parser uses the standard parsers from the HL7 SDK. The SDK contains separate parsers for Json and Xml that are used as follows:
```csharp
var bundle = new FhirJsonParser().Parse<Bundle>(json)
```
```csharp
var bundle = new FhirXmlParser().Parse<Bundle>(xml)
```
Both return a generic Fhir POCO (in this case a Bundle). The class library in this sample also implement and use a number of mappers that shows how to extract (most of) the CareCommunication message to DTO.

### Extracting from a POCO
An example of this is how to map a Fhir POCO to an organization DTO. The POCO representing the organization contains the expected attributes specified by the standard as illustrated by the following snippet.
```csharp
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
```

### Extensions and References
The following snippet illustrates how to extract the author extension value from the payload component of the message and handle the reference. The extension value is extracted using GetExtensionValue<T> where T is the type of the value. In this case we extract a resource reference which points to a resource in the bundle.
```csharp
private const string AuthorExtension = "http://medcomfhir.dk/fhir/core/1.0/StructureDefinition/medcom-core-author-extension";

private string MapAuthorDisplayName(PayloadComponent textPayloadComponent, Func<string?, Resource?> resourceLocator)
{
    var fhirAuthorReference = textPayloadComponent.GetExtensionValue<ResourceReference>(AuthorExtension);
    if (resourceLocator(fhirAuthorReference.Reference) is not Practitioner fhirAuthor)
        throw new InvalidOperationException("Referenced Practitioner resource not found");

    return ConvertHumanNameToText(fhirAuthor.Name);
}
```
The resourceLocator function passed to the method locates the resource in the bundle and is defined previously (where the entire message bundle is in scope) as follows:
```csharp
var resourceLocator = (string? reference) => message.Entry.FirstOrDefault(e => e.FullUrl == reference)?.Resource;
```
It simply looks up the matching resource in the Entry collection in the message bundle.

## Serializing to Fhir
The CareCommunication serializer uses the standard serializers from the HL7 SDK. The SDK contains separate serializers for Json and Xml that are used as follows:
```csharp
var json = new FhirJsonSerializer().SerializeToString(bundle);
```
```csharp
var xml = new FhirXmlSerializer().SerializeToString(bundle);
```
Both take a POCO as input (in this case a bundle) and returns the Fhir message in Json and Xml respectively. The class library in this sample also implement and use a number of mappers that shows how to serialize (most of) the CareCommunication message from a DTO.

### Building a POCO
An example of this is how to map an organization DTO to a Fhir Organization POCO. The POCO is simply newed and the attributes are set to the values from the DTO. The POCO contains the expected attributes specified by the standard as illustrated by the following snippet.
```csharp
public Organization Map(MessagingOrganizationDTO primaryReceiver)
{
    var fhirOrganization = new Organization();

    fhirOrganization.Id = Guid.NewGuid().ToString();
    fhirOrganization.Name = primaryReceiver.Name;
    fhirOrganization.Identifier.Add(new Identifier(SOR, primaryReceiver.SOR));
    fhirOrganization.Identifier.Add(new Identifier(EAN, primaryReceiver.EAN));

    return fhirOrganization;
}
```

### Extensions and References
The following snippets illustrates how to compose author extension values to the payload component of the message and handle the reference to the author. In this case we compose a resource reference which points to a resource that we append to the bundle.
```csharp
private const string AuthorExtension = "http://medcomfhir.dk/fhir/core/1.0/StructureDefinition/medcom-core-author-extension";

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
```

The extension is added using the AddExtension() method on the component.
```csharp
var textPayloadComponent = new PayloadComponent();
textPayloadComponent.AddExtension(AuthorExtension, MapAuthorDisplayName(payloadText.Author, resourceAppender));
```

The resourceAppender function passed to the method appends the resource to the bundle and is defined previously (where the entire message bundle is in scope) as follows:
```csharp
Action<Resource, string> resourceAppender = (resource, url) => message.AddResourceEntry(resource, url);
```
It simply adds resource along with its url in the Entry collection in the message bundle.

## Validating Fhir data
Validation is not necessarily needed in a prject, but this test project validates the serialized fhir data against the profile to chect whether the serialized message comply with all the rules in the implementation guide.

The following code setup and validate a serialized message with Fhir R4 and the implementation guide from MedCom. The zipFile is downloaded from http://build.fhir.org/ig/hl7dk/kl-medcom/definitions.xml.zip.
```csharp
var zipFile = @"..\..\..\Definitions\definitions.xml.zip";

var resolvers = new List<IAsyncResourceResolver>();
resolvers.Add(new CachedResolver(ZipSource.CreateValidationSource()));
resolvers.Add(new CachedResolver(new ZipSource(zipFile)));

settings = ValidationSettings.CreateDefault();
var multiResolver = new MultiResolver(resolvers);

settings.ResourceResolver = multiResolver;
settings.GenerateSnapshot = true;

var operationOutcome = validator.Validate(message);
```
The resulting operationOutcome contains a list of all issues in the message. The message is valid if operationOutcome.Error is 0.