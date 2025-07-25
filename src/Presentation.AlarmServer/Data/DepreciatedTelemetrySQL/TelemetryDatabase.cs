using Application.CustomFields.Dtos;
using Application.Triggers;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Presentation.AlarmServer.Email;
using Presentation.AlarmServer.Helpers;
using Presentation.AlarmServer.Models;
using Presentation.AlarmServer.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;

namespace Presentation.AlarmServer.Data.TelemetrySQL;

public class TelemetryDatabase(ISqlConnectionProvider sqlConnectionProvider, ILoggerFactory loggerFactory) : ITelemetryDatabase
{
    protected readonly ILogger logger = loggerFactory.CreateLogger<TelemetryDatabase>();

    public Reading GetMostRecentReadingBefore(int sensorId, DateTime thisReadingDate)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using var cmd = cn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "GetMostRecentReadingBefore";
        cmd.Parameters.AddWithValue("@sensorId", sensorId);
        cmd.Parameters.AddWithValue("@thisReadingDate", thisReadingDate);

        using var r = cmd.ExecuteReader();
        if (!r.Read())
            return null;

        return new Reading
        {
            SensorId = sensorId,
            Value = r.GetDouble(0),
            DateRecordedUtc = r.GetDateTime(1)
        };
    }
    public IList<AlarmServerDto> GetNewReadingsWithAlarms()
    {
        IList<AlarmServerDto> alarmsToHandle = [];
        var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetNewReadingsWithAlarms";
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    // If we get here, we should handle it.
                    var newitem = new AlarmServerDto
                    {
                        SensorId = r.GetInt32(0),
                        AlarmType = (TriggerTypeCode)r.GetChar(1),
                        AlarmValue = SqlHelper.DoubleOrNull(r, 2),
                        RecipientMode = RecipientModeHelper.FromString(r.GetString(3)),
                        AlarmSubject = SqlHelper.StringOrNull(r, 4),
                        AlarmBody = SqlHelper.StringOrNull(r, 5),
                        Value = SqlHelper.DoubleOrNull(r, 6),
                        IsAlarm = r.GetBoolean(7),
                        DateRecordedUtc = r.GetDateTime(8),
                        PendingAlarmTriggerId = r.GetInt32(9),
                        AlarmSetId = r.GetInt32(10),
                        AlarmId = r.GetInt32(11),
                        MinimumSendIntervalMinutes = r.GetInt32(12)
                    };
                    alarmsToHandle.Add(newitem.InitialiseKnownValues());
                }
            }
        }
        return alarmsToHandle;
    }


    /// <summary>
    ///     Return true if this alarm has already been sent within its quench period; false otherwise.
    /// </summary>
    /// <returns>true if this alarm has already been sent within its quench period; false otherwise</returns>
    public bool IsQuenched(AlarmServerDto trigger)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandText =
            "select MostRecentSendUtc from MostRecentAlarms where SensorID = @sensorId and AlarmID = @alarmId";
            cmd.Parameters.AddWithValue("@sensorId", trigger.SensorId);
            cmd.Parameters.AddWithValue("@alarmId", trigger.AlarmId);

            using var r = cmd.ExecuteReader();
            if (r.Read() && !r.IsDBNull(0))
            {
                var mostRecentSendUtc = r.GetDateTime(0);
                var minutesSinceLastSend = (DateTime.UtcNow - mostRecentSendUtc).TotalMinutes;
                logger.LogDebug("Is quenched? {MinutesSinceLastSend} {MinimumSendIntervalMinuted}", minutesSinceLastSend, trigger.MinimumSendIntervalMinutes);
                return minutesSinceLastSend < trigger.MinimumSendIntervalMinutes;
            }
        }

        logger.LogDebug("No previous alarm, so not quenched");
        return false;
    }

    public void NoteAlarmTrigger(AlarmServerDto trigger, bool fired)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "NoteAlarmTrigger";
            cmd.Parameters.AddWithValue("@alarmId", trigger.AlarmId);
            cmd.Parameters.AddWithValue("@sensorId", trigger.SensorId);
            cmd.Parameters.AddWithValue("@fired", fired);
            cmd.ExecuteNonQuery();
        }
    }
    public void NoteAlarmNotTriggered(AlarmServerDto trigger)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "NoteAlarmNotTriggered";

            var sensorIdParameter = cmd.CreateParameter();
            sensorIdParameter.ParameterName = "@sensorId";
            sensorIdParameter.Value = trigger.SensorId;
            cmd.Parameters.Add(sensorIdParameter);

            var alarmIdParameter = cmd.CreateParameter();
            alarmIdParameter.ParameterName = "@alarmId";
            alarmIdParameter.Value = trigger.AlarmId;
            cmd.Parameters.Add(alarmIdParameter);

            cmd.ExecuteNonQuery();
        }
    }
    public void AcknowledgeProcessing(int pendingAlarmTriggerId)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AcknowledgeProcessing";
            cmd.Parameters.AddWithValue("@pendingAlarmTriggerId", pendingAlarmTriggerId);
            cmd.ExecuteNonQuery();
        }
    }
    public void CalculateSensor(BreachedTriggerDto triggerDto)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandText =
            "select Channel, LowValue, HighValue, EngineeringUnits, SensorName from Sensors where SensorID = @sensorId";
            cmd.Parameters.AddWithValue("@sensorId", triggerDto.SensorId);

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                triggerDto.SetKnownValue("channel", (int)r.GetByte(0));
                triggerDto.SetKnownValue("lowvalue", r.IsDBNull(1) ? null : r.GetInt32(1));
                triggerDto.SetKnownValue("highvalue", r.IsDBNull(2) ? null : r.GetInt32(2));
                triggerDto.SetKnownValue("engineeringunits", r.IsDBNull(3) ? null : r.GetString(3));
                triggerDto.SetKnownValue("sensorname", r.IsDBNull(4) ? null : r.GetString(4));
            }
        }
    }
    public void CalculateRtu(AlarmServerDto alarmTriggerTemplateValues)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandText =
                "select Sensors.RTUID, PhoneNumber, ManufacturerID, ClientID from RTUs inner join Sensors on RTUs.RTUID = Sensors.RTUID where SensorID = @sensorId";
            cmd.Parameters.AddWithValue("@sensorId", alarmTriggerTemplateValues.SensorId);
            using (var r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    alarmTriggerTemplateValues.SetKnownValue("rtuid", r.GetInt32(0));
                    alarmTriggerTemplateValues.SetKnownValue("phonenumber", r.IsDBNull(1) ? null : r.GetString(1));
                    alarmTriggerTemplateValues.SetKnownValue("manufacturerid", r.IsDBNull(2) ? null : r.GetString(2));
                    alarmTriggerTemplateValues.SetKnownValue("clientid", r.IsDBNull(3) ? null : r.GetString(3));
                }
            }
        }
    }
    public bool CalculateV2CustomerDataForSensor(AlarmServerDto alarmTrigger)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandText = @"
                    SELECT R.CustomFieldValues 
                    FROM dbo.RTUs R 
                        INNER JOIN dbo.Sensors S ON R.RTUID = S.RTUID 
                    WHERE S.SensorID = @SensorId";
            cmd.Parameters.AddWithValue("@SensorId", alarmTrigger.SensorId);

            var valuesBlob = cmd.ExecuteScalar() as string;

            if (valuesBlob == null)
            {
                logger.LogDebug("No field values defined");
                return false;
            }

            using var definitionsCms = cn.CreateCommand();
            definitionsCms.CommandText = @"
                    SELECT TOP 1 ParentCompany.CustomFieldDefinitions
                    FROM dbo.Companies ParentCompany
                        INNER JOIN dbo.CompaniesAndDescendants
                            ON ParentCompany.CompanyID = CompaniesAndDescendants.AncestorCompanyID
                        INNER JOIN dbo.RTUs RU ON CompaniesAndDescendants.DescendantCompanyID = RU.CompanyID 
                        INNER JOIN dbo.Sensors S ON RU.RTUID = S.RTUID 
                    WHERE S.SensorID = @SensorId
                    AND ParentCompany.CustomFieldDefinitions IS NOT NULL
                    ORDER BY CompaniesAndDescendants.Level";
            definitionsCms.Parameters.AddWithValue("@SensorId", alarmTrigger.SensorId);

            var definitionsBlob = definitionsCms.ExecuteScalar() as string;

            if (definitionsBlob == null)
            {
                logger.LogDebug("No custom fields defined");
                return true;
            }


            try
            {
                logger.LogTrace("Loading custom field definitions from {ValuesBlob}", definitionsBlob);
                var parsedDefinitions = JArray.Parse(definitionsBlob);
                var definitions= new Dictionary<string, string>();
                if (parsedDefinitions != null)
                {
                    definitions = parsedDefinitions!.Select(definitionJObject => new
                    {
                        Id = definitionJObject.Value<string>("Id")!,
                        Title = definitionJObject.Value<string>("Name")
                    }).ToDictionary(d => d.Id, d => TitleForAlarm(d.Title!));
                }


                logger.LogTrace("Loading custom field values from {ValuesBlob}", valuesBlob);
                var customFieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(valuesBlob)!;
                foreach (var keyValuePair in customFieldValues)
                {
                    if (definitions.TryGetValue(keyValuePair.Key, out var title))
                    {
                        logger.LogTrace("Setting custom field value {Key} to {Value}", title, keyValuePair.Value);
                        alarmTrigger.SetKnownValue(title, keyValuePair.Value);
                    }
                    else
                    {
                        logger.LogTrace("Unknown custom field {@Value}", keyValuePair);
                    }
                }
            }
            catch (JsonReaderException o)
            {
                logger.LogWarning(o, "Cannot decipher Custom Fields Json {Definition} and values {Values}", definitionsBlob, valuesBlob);
            }
        }

        return true;
    }
    public void CalculateV1CustomDataForSensor(AlarmServerDto alarmTrigger)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandText = "GetCustomDataForSensor";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@sensorId", alarmTrigger.SensorId);

            using var r = cmd.ExecuteReader();
            // Part 1: one row per variable name and type
            var titleToFieldMap = new Dictionary<string, string>();
            while (r.Read())
            {
                var title = r.GetString(0);
                var dataType = r.GetString(1);
                var fieldIndex = r.GetByte(2);
                var titleForAlarm = TitleForAlarm(title);
                var dataFieldPrefix = "C".Equals(dataType) ? "Char" : "Float";
                var dataFieldSuffix = (char)('A' + fieldIndex);
                titleToFieldMap.Add(titleForAlarm, dataFieldPrefix + dataFieldSuffix);
            }

            // Part 2: Data values.  All come back; we cherry-pick.
            if (r.NextResult() && r.Read())
                foreach (var fieldPair in titleToFieldMap)
                {
                    var fieldPos = r.GetOrdinal(fieldPair.Value);
                    alarmTrigger.SetKnownValue(fieldPair.Key, r.IsDBNull(fieldPos) ? null : r.GetValue(fieldPos));
                }
        }
    }
    public ICollection<MailAddressAndFroms> GetEmailToAddresses(AlarmServerDto trigger)
    {
        logger.LogTrace("Get Email Addresses for AlarmSetId={AlarmSetId}", trigger.AlarmSetId);
        ICollection<MailAddressAndFroms> addresses = new List<MailAddressAndFroms>();
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetEmailToAddressesForAlarmSet";
            cmd.Parameters.AddWithValue("@alarmSetId", trigger.AlarmSetId);


            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var toEmailAddress = r.GetString(1);
                var toDisplayName = r.GetString(0);
                MailAddress to;
                try
                {
                    to = new MailAddress(toEmailAddress, toDisplayName);
                }
                catch (FormatException)
                {
                    logger.LogError("To {EmailAddress} is not a valid email address ", toEmailAddress);
                    continue;
                }

                var from = r.IsDBNull(2) ? null : r.GetString(2);
                try
                {
                    addresses.Add(new MailAddressAndFroms
                    {
                        To = to,
                        From = from,
                        ReplyTo = r.IsDBNull(3) ? null : r.GetString(3)
                    });
                }
                catch (FormatException)
                {
                    logger.LogError("From {EmailAddress} is not a valid email address ", from);
                }
            }
        }
        logger.LogTrace("Got {Count} Email Addresses for AlarmSetId={AlarmSetId}", addresses.Count, trigger.AlarmSetId);

        return addresses;
    }
    public ICollection<SmsRecipientTemplates> GetSmsToAddresses(AlarmServerDto trigger)
    {
        ICollection<SmsRecipientTemplates> addresses = new List<SmsRecipientTemplates>();
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSmsAddressesForAlarmSet";
            cmd.Parameters.AddWithValue("@alarmSetId", trigger.AlarmSetId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                addresses.Add(new SmsRecipientTemplates
                {
                    Sms = r.GetString(0),
                    ToAddressTemplate = r.IsDBNull(1) ? null : r.GetString(1),
                    SubjectTemplate = r.IsDBNull(2) ? null : r.GetString(2),
                    BodyTemplate = r.IsDBNull(3) ? null : r.GetString(3),
                    FromAddress = r.IsDBNull(4) ? null : r.GetString(4),
                    CompanyId = r.GetInt32(5),
                    RecipientId = r.GetInt32(6)
                });
            }
        }

        return addresses;
    }

    /// <summary>
    /// All alarm variables are searched as if they are lowercase, and spaces are removed.  This puts the title into that form.
    /// </summary>
    private string TitleForAlarm(string title) => title.ToLower().Replace(" ", "");
    public IList<AlarmServerDto> GetNotReportedReadingsWithAlarms()
    {
        var alarmsToHandle = new List<AlarmServerDto>();
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetNotReportedReadingsWithAlarms";
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {

                //MW - left here for diagnostic purposes until we can add some tests!
                //var cc = r.GetInt32(0);
                //var  mm = AlarmType.FromAlarmTypeId(r.GetString(1));
                //var f1 = SqlHelper.DoubleOrNull(r, 2);
                //var f2 = RecipientModeHelper.FromString(r.GetString(3));
                //var f3 = SqlHelper.StringOrNull(r, 4);
                //var f4 = SqlHelper.StringOrNull(r, 5);
                //var f5 = SqlHelper.DoubleOrNull(r, 6);
                //var f6 = SqlHelper.BooleanOrNull(r,7);
                //var f7 = SqlHelper.DateTimeOrNull (r,8);
                //var f8 = SqlHelper.Int32OrNull(r, 9);
                //var f9 = SqlHelper.Int32OrNull(r, 10);
                //var f10 = SqlHelper.Int32OrNull(r, 11);
                //var f11 = SqlHelper.Int32OrNull(r, 12);
                // If we get here, we should handle it.
                var newitem = new AlarmServerDto
                {
                    SendAlarmForNotReported = true,
                    SensorId = r.GetInt32(0),
                    AlarmType = (TriggerTypeCode)r.GetChar(1),
                    AlarmValue = SqlHelper.DoubleOrNull(r, 2),
                    RecipientMode = RecipientModeHelper.FromString(r.GetString(3)),
                    AlarmSubject = SqlHelper.StringOrNull(r, 4),
                    AlarmBody = SqlHelper.StringOrNull(r, 5),
                    Value = SqlHelper.DoubleOrNull(r, 6),
                    IsAlarm = SqlHelper.BooleanOrNull(r, 7),
                    DateRecordedUtc = SqlHelper.DateTimeOrNull(r, 8),
                    PendingAlarmTriggerId = SqlHelper.Int32OrNull(r, 9),
                    AlarmSetId = SqlHelper.Int32OrNull(r, 10),
                    AlarmId = SqlHelper.Int32OrNull(r, 11),
                    MinimumSendIntervalMinutes = SqlHelper.Int32OrNull(r, 12)
                };
                alarmsToHandle.Add(newitem.InitialiseKnownValues());
            }
        }
        return alarmsToHandle;
    }
    public IList<HenkelRtuStatus> GetEnabledCustomFieldValues(WorkerOptions workerOptions)
    {
        logger.LogDebug("Processing GetEnabledCustomFieldValues");
        using var cn = sqlConnectionProvider.GetOpenConnection();
        IList<HenkelRtuStatus> rtus = new List<HenkelRtuStatus>();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetEnabledCustomFieldValue";

            var isEnabledFieldIdParameter = cmd.CreateParameter();
            isEnabledFieldIdParameter.ParameterName = "@enabledFieldId";
            isEnabledFieldIdParameter.Value = workerOptions.HenkelUseSpiderScopeStatusCustomFieldName;
            cmd.Parameters.Add(isEnabledFieldIdParameter);

            var valueFieldIdParameter = cmd.CreateParameter();
            valueFieldIdParameter.ParameterName = "@valueFieldId";
            valueFieldIdParameter.Value = workerOptions.HenkelSpiderScopeStatusCustomFieldName;
            cmd.Parameters.Add(valueFieldIdParameter);

            var isEnabledValueParameter = cmd.CreateParameter();
            isEnabledValueParameter.ParameterName = "@enabledValue";
            isEnabledValueParameter.Value = "1";
            cmd.Parameters.Add(isEnabledValueParameter);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    // If we get here, we should handle it.
                    rtus.Add(new()
                    {
                        RtuId = SqlHelper.Int32OrNull(r, 0),
                        ManufacturerId = SqlHelper.StringOrNull(r, 1),
                        Status = SqlHelper.StringOrNull(r, 2)
                    });
            }
        }

        return rtus; ;
    }
    public IList<int> GetRTUsWithCustomFieldValue(string customFieldId, string customFieldValue="1")
    {
        logger.LogDebug("Processing GetCustomFieldValue");
        using var cn = sqlConnectionProvider.GetOpenConnection();
        IList<int> rtus = new List<int>();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCustomFieldValue";

            var isEnabledFieldIdParameter = cmd.CreateParameter();
            isEnabledFieldIdParameter.ParameterName = "@fieldId";
            isEnabledFieldIdParameter.Value = customFieldId;
            cmd.Parameters.Add(isEnabledFieldIdParameter);

            var isEnabledValueParameter = cmd.CreateParameter();
            isEnabledValueParameter.ParameterName = "@enabledValue";
            isEnabledValueParameter.Value = customFieldValue;
            cmd.Parameters.Add(isEnabledValueParameter);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    // If we get here, we should handle it.
                    rtus.Add(SqlHelper.Int32OrNull(r, 0));
            }
        }

        return rtus; ;
    }
    public void SetHenkelStatusCustomFieldValueCache(HenkelRtuStatus rtuCustomField, HenkelStatusType newStatus, WorkerOptions workerOptions)
    {
        //queue a configuration
        logger.LogDebug("Processing SetHenkelStatusCustomFieldValueCache");
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SetCustomFieldValue";

            var RTUIDParameter = cmd.CreateParameter();
            RTUIDParameter.ParameterName = "@rtuId";
            RTUIDParameter.Value = rtuCustomField.RtuId;
            cmd.Parameters.Add(RTUIDParameter);

            var fieldIdParameter = cmd.CreateParameter();
            fieldIdParameter.ParameterName = "@fieldId";
            fieldIdParameter.Value = workerOptions.HenkelSpiderScopeStatusCustomFieldName;
            cmd.Parameters.Add(fieldIdParameter);

            var valueIdParameter = cmd.CreateParameter();
            valueIdParameter.ParameterName = "@value";
            valueIdParameter.Value = newStatus.ToString();
            cmd.Parameters.Add(valueIdParameter);

            cmd.ExecuteNonQuery();
        }
    }
    public void AddConfigrationUpload(HenkelRtuStatus enabledRtu, string configuration)
    {
        using var cn = sqlConnectionProvider.GetOpenConnection();
        using (var cmd = cn.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AddConfigurationUpload";

            var RTUIDParameter = cmd.CreateParameter();
            RTUIDParameter.ParameterName = "@RTUID";
            RTUIDParameter.Value = enabledRtu.RtuId;
            cmd.Parameters.Add(RTUIDParameter);

            var configurationParameter = cmd.CreateParameter();
            configurationParameter.ParameterName = "@Configuration";
            configurationParameter.Value = configuration;
            cmd.Parameters.Add(configurationParameter);

            var queuingUserNameParameter = cmd.CreateParameter();
            queuingUserNameParameter.ParameterName = "@QueuingUserName";
            queuingUserNameParameter.Value = "SpiderScopeAPI";
            cmd.Parameters.Add(queuingUserNameParameter);

            cmd.ExecuteNonQuery();
        }
    }

    public void CalculateSensor(AlarmServerDto triggerDto)
    {
        throw new NotImplementedException();
    }

    public ICollection<SmsRecipientTemplates> GetSmsToAddresses(BreachedTriggerDto trigger)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<MailAddressAndFroms> GetEmailToAddresses(BreachedTriggerDto triggerDto)
    {
        throw new NotImplementedException();
    }

    //public void AddConfigrationUpload(HenkelRtuStatus enabledRtu, string configuration)
    //{
    //    throw new NotImplementedException();
    //}

    //IList<HenkelRtuStatus> ITelemetryDatabase.GetEnabledCustomFieldValues(WorkerOptions workerOptions)
    //{
    //    throw new NotImplementedException();
    //}

    //public void SetHenkelStatusCustomFieldValueCache(HenkelRtuStatus rtuCustomField, HenkelStatusType newStatus, WorkerOptions workerOptions)
    //{
    //    throw new NotImplementedException();
    //}
}
