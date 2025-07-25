using Application.Triggers;
using Presentation.AlarmServer.Models;

namespace Presentation.AlarmServer.Alarms;
public interface ISensorServerService
{
    object CalculateTemplatePlaceholderValue(BreachedTriggerDto alarm, string fieldName);
    string Substitute(BreachedTriggerDto alarm, string templateSnippet);
}