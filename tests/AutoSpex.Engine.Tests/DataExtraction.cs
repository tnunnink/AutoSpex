using System.Text;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class DataExtraction
{
    [Test]
    public void GetAnalogAlarmsDataFromFile()
    {
        var content = L5X.Load(Known.Example);

        var tags = content.Query<Tag>()
            .Where(t => t.DataType.Equals("AnalogInput") || t.DataType.Equals("AnalogOutput"))
            .SelectMany(t => t.Members())
            .Where(t => t.Value is AtomicData)
            .Where(t => t.Value > 0)
            .Where(t => t.TagName.Contains("AlmSP") || t.TagName.Contains("Enabled"))
            .Select(t => new { t.TagName, t.Value, t.Description})
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("TagName,Value,Description");
        
        foreach (var tag in tags)
        {
            builder.AppendLine($"{tag.TagName},{tag.Value},{tag.Description}");
        }
        
        File.WriteAllText(@"C:\Users\tnunn\Documents\Test\AnalogAlarmSetpoints.csv", builder.ToString());
    }
}