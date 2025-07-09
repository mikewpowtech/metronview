using Presentation.AlarmServer.Email;
using Presentation.AlarmServer.Enums;
using Presentation.AlarmServer.Helpers;
using Presentation.AlarmServer.Models;
using Presentation.AlarmServer.Options;
using System;
using System.Collections.Generic;

namespace Presentation.AlarmServer.Data.TelemetrySQL;

public interface ITelemetryDatabase
{
    void AcknowledgeProcessing(int pendingAlarmTriggerId);
    void AddConfigrationUpload(HenkelRtuStatus enabledRtu, string configuration);
    void CalculateRtu(AlarmServerDto alarmTriggerTemplateValues);
    void CalculateSensor(AlarmServerDto triggerDto);
    void CalculateV1CustomDataForSensor(AlarmServerDto alarmTrigger);
    bool CalculateV2CustomerDataForSensor(AlarmServerDto alarmTrigger);
    ICollection<MailAddressAndFroms> GetEmailToAddresses(AlarmServerDto trigger);
    IList<HenkelRtuStatus> GetEnabledCustomFieldValues(WorkerOptions workerOptions);
    Reading GetMostRecentReadingBefore(int sensorId, DateTime thisReadingDate);
    IList<AlarmServerDto> GetNewReadingsWithAlarms();
    IList<AlarmServerDto> GetNotReportedReadingsWithAlarms();
    ICollection<SmsRecipientTemplates> GetSmsToAddresses(AlarmServerDto trigger);
    bool IsQuenched(AlarmServerDto trigger);
    void NoteAlarmNotTriggered(AlarmServerDto trigger);
    void NoteAlarmTrigger(AlarmServerDto trigger, bool fired);
    void SetHenkelStatusCustomFieldValueCache(HenkelRtuStatus rtuCustomField, HenkelStatusType newStatus, WorkerOptions workerOptions);
}