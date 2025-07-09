using Presentation.AlarmServer.Models;

namespace Presentation.AlarmServer.Alarms;
public interface ISensorServerService
{
    object CalculateTemplatePlaceholderValue(AlarmServerDto alarm, string fieldName);
    string Substitute(AlarmServerDto alarm, string templateSnippet);
}