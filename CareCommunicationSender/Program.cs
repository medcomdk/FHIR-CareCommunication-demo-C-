
using CareCommunicationProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

var outputDir = args.FirstOrDefault(s => s.StartsWith("/O:"))?.Substring(3) ?? ".";

Console.Write("Sender Name:");
var senderName = Console.ReadLine() ?? String.Empty;
Console.Write("  - SOR code:");
var senderSORCode = Console.ReadLine() ?? String.Empty;
Console.Write("  - EAN code:");
var senderEANCode = Console.ReadLine() ?? String.Empty;
var sender = new MessagingOrganizationDTO(senderName, senderSORCode, senderEANCode);

Console.Write("Receiver Name:");
var receiverName = Console.ReadLine() ?? String.Empty;
Console.Write("  - SOR code:");
var receiverSORCode = Console.ReadLine() ?? String.Empty;
Console.Write("  - EAN code:");
var receiverEANCode = Console.ReadLine() ?? String.Empty;
var receiver = new MessagingOrganizationDTO(receiverName, receiverSORCode, receiverEANCode);

Console.Write("Patient Name:");
var patientName = Console.ReadLine() ?? String.Empty;
Console.Write("  - Address (street):");
var addressStreet = Console.ReadLine() ?? String.Empty;
Console.Write("  - Address (postal code):");
var addressPostalCode = Console.ReadLine() ?? String.Empty;
Console.Write("  - Address (city):");
var addressCity = Console.ReadLine() ?? String.Empty;
var address = new AddressDTO(addressStreet, addressPostalCode, addressCity);
Console.Write("  - Deseased (Y/N):");
var deseased = Console.ReadLine()?.First() == 'Y';
var patient = new PatientDTO(patientName, new List<AddressDTO> { address }, deseased);

var category = "carecoordination";
var topic = (string?)null;

var timestamp = DateTimeOffset.Now;
var author = "Michael Burns";
Console.Write("Text:");
var text = Console.ReadLine() ?? String.Empty;
var payloadTexts = new List<PayloadTextDTO>() { new PayloadTextDTO(timestamp, author, text) };

var careCommunication = new CareCommunicationDTO(patient, category, topic, payloadTexts);
var message = new CareCommunicationMessageDTO(sender, receiver, careCommunication);

var json = new CareCommunicationSerializer().SerializeAsJson(message);

File.WriteAllText($"{outputDir}\\msg{timestamp.LocalDateTime.ToString("yyyyMMddHHmmssfff")}.json", json);
