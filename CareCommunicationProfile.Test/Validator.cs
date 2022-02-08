using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using System.Collections.Generic;

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
        return validator.Validate(message, "http://medcomfhir.dk/fhir/core/1.0/StructureDefinition/medcom-careCommunication-message");
    }

}