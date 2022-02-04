
using CareCommunicationProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

var outputDir = args.FirstOrDefault() ?? ".";

Console.Write("Sender Name:");
var senderName = GetInputString("Sender Name");
Console.Write("  - SOR Code:");
var senderSORCode = GetInputString("123456789012345");
Console.Write("  - EAN Code:");
var senderEANCode = GetInputString("123456789012");
var sender = new MessagingOrganizationDTO(senderName, senderSORCode, senderEANCode);

Console.Write("Receiver Name:");
var receiverName = GetInputString("Receiver Name");
Console.Write("  - SOR Code:");
var receiverSORCode = GetInputString("0987654321098765");
Console.Write("  - EAN Code:");
var receiverEANCode = GetInputString("098765432109");
var receiver = new MessagingOrganizationDTO(receiverName, receiverSORCode, receiverEANCode);

Console.Write("Patient Name:");
var patientName = GetInputString("Patient Name");
Console.Write("  - Address (street):");
var addressStreet = GetInputString("Street Name X");
Console.Write("  - Address (postal code):");
var addressPostalCode = GetInputString("Postal Code");
Console.Write("  - Address (city):");
var addressCity = GetInputString("City");
var address = new AddressDTO(addressStreet, addressPostalCode, addressCity);
Console.Write("  - Deseased (Y/N):");
var deseased = GetInputString("Y").ToUpper().StartsWith('Y');
var patient = new PatientDTO(patientName, new List<AddressDTO> { address }, deseased);

var category = "carecoordination";
var topic = (string?)null;

var timestamp = DateTimeOffset.Now;
var author = "Michael Burns";
Console.Write("Text:");
var text = GetInputString("Text content");
var payloadTexts = new List<PayloadTextDTO>() { new PayloadTextDTO(timestamp, author, text) };

var careCommunication = new CareCommunicationDTO(patient, category, topic, payloadTexts);
var message = new CareCommunicationMessageDTO(timestamp, sender, receiver, careCommunication);

try
{
    var fileName = $"{outputDir}\\msg{timestamp.LocalDateTime.ToString("yyyyMMddHHmmssfff")}.json";
    var json = CareCommunicationSerializer.SerializeAsJson(message);

    File.WriteAllText(fileName, json);

    Console.WriteLine($"Generated file: {fileName}");

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");

    return -1;
}

static string GetInputString(string defaultValue)
{
    var result = Console.ReadLine();
    return string.IsNullOrWhiteSpace(result) ? defaultValue : result;
}