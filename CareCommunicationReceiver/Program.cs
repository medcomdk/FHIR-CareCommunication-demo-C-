using CareCommunicationProfile;
using System;
using System.IO;
using System.Linq;

var msgFile = args.FirstOrDefault() ?? string.Empty;

if (msgFile == string.Empty)
{
    Console.WriteLine("Usage: CareCommunicationReceiver <file name>");
    return -1;
}

try
{
    var messageText = File.ReadAllText(msgFile);

    var msg = CareCommunicationParser.FromJson(messageText);

    Console.WriteLine($"Sender Name: {msg.Sender.Name}");
    Console.WriteLine($"  - SOR Code: {msg.Sender.SOR}");
    Console.WriteLine($"  - EAN Code: {msg.Sender.EAN}");
    Console.WriteLine();
    Console.WriteLine($"Receiver Name: {msg.PrimaryReceiver.Name}");
    Console.WriteLine($"  - SOR Code: {msg.PrimaryReceiver.SOR}");
    Console.WriteLine($"  - EAN Code: {msg.PrimaryReceiver.EAN}");
    Console.WriteLine();
    Console.WriteLine($"Patient Name: {msg.CareCommunication.Subject.Name}");
    Console.WriteLine($"  - Address (street): {msg.CareCommunication.Subject.Address.FirstOrDefault()?.Line ?? String.Empty}");
    Console.WriteLine($"  - Address (postal code): {msg.CareCommunication.Subject.Address.FirstOrDefault()?.PostalCode ?? String.Empty}");
    Console.WriteLine($"  - Address (city): {msg.CareCommunication.Subject.Address.FirstOrDefault()?.City ?? String.Empty}");
    Console.WriteLine($"  - Deseased: {(msg.CareCommunication.Subject.Deceased ? "Yes" : "No")}");
    Console.WriteLine();
    Console.WriteLine($"Category: {msg.CareCommunication.Category}");
    Console.WriteLine($"Topic: {msg.CareCommunication.Topic ?? String.Empty}");
    Console.WriteLine();
    Console.WriteLine($"Timestamp: {msg.CareCommunication.PayloadTexts.FirstOrDefault()?.Timestamp.ToString() ?? string.Empty}");
    Console.WriteLine($"Author: {msg.CareCommunication.PayloadTexts.FirstOrDefault()?.Author ?? string.Empty}");
    Console.WriteLine($"Text: {msg.CareCommunication.PayloadTexts.FirstOrDefault()?.Text ?? string.Empty}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

return 0;
