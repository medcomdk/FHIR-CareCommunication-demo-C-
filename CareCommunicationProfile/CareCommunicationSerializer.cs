using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareCommunicationProfile;

public class CareCommunicationSerializer
{
    public static string SerializeAsXML(CareCommunicationMessageDTO messageDTO) =>
        new FhirXmlSerializer().SerializeToString(new CareCommunicationMessageMapper().Map(messageDTO));

    public static string SerializeAsJson(CareCommunicationMessageDTO messageDTO) =>
        new FhirJsonSerializer().SerializeToString(new CareCommunicationMessageMapper().Map(messageDTO));
}

