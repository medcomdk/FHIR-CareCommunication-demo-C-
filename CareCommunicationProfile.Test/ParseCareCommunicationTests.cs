using System.IO;
using Xunit;

namespace CareCommunicationProfile.Test;

public class ParseCareCommunicationTests
{
    [Fact]
    public void ParseNewMessageFromXml()
    {
        var path = @"..\..\..\Messages\NewMessage.xml";

        var message = File.ReadAllText(path);


        var messageDTO = CareCommunicationParser.FromXml(message);

        var patient = messageDTO.CareCommunication.Subject;
        var payloadTexts = messageDTO.CareCommunication.PayloadTexts;
    }

    [Fact]
    public void ParseNewMessageFromJson()
    {
        var path = @"..\..\..\Messages\NewMessage.json";

        var message = File.ReadAllText(path);

        var messageDTO = CareCommunicationParser.FromJson(message);

        var patient = messageDTO.CareCommunication.Subject;
        var payloadTexts = messageDTO.CareCommunication.PayloadTexts;
    }
}
