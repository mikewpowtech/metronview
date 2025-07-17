using Application.Alarms;
using Application.Triggers;
using Application.Units;
using Domain;
using Microsoft.Extensions.Logging;
using Substituter;
using System.Text;

namespace Application.Sensors;

public class SensorService : ISensorService
{
    private readonly ISensorRepository sensorRepository;
    private readonly IUnitRepository unitRepository;
    private readonly ILogger<SensorService> logger;

    public SensorService(ISensorRepository sensorRepository,IUnitRepository unitRepository, ILogger<SensorService> logger)
    {
        this.sensorRepository = sensorRepository;
        this.unitRepository = unitRepository;
        this.logger = logger;
    }

    public Task<Sensor?> GetByIdAsync(int id) => sensorRepository.GetByIdAsync(id);

    public Task<List<Sensor>> GetByUnitIdAsync(int unitId) => sensorRepository.GetByUnitIdAsync(unitId);

    public Task<List<Sensor>> GetByCompanyIdAsync(int companyId) => sensorRepository.GetByCompanyIdAsync(companyId);

    public Task<List<Sensor>> GetByAlarmIdAsync(int alarmId) => sensorRepository.GetByAlarmIdAsync(alarmId);

    public Task<List<Sensor>> GetAllAsync() => sensorRepository.GetAllAsync();

    public Task<Sensor> AddAsync(Sensor sensor) => sensorRepository.AddAsync(sensor);

    public Task<bool> UpdateAsync(Sensor sensor) => sensorRepository.UpdateAsync(sensor);

    public Task<bool> DeleteAsync(int id) => sensorRepository.DeleteAsync(id);

    public async Task CalculateSensorAsync(BreachedTriggerDto triggerDto)
    {
        var sensor = await sensorRepository.GetByIdAsync(triggerDto.SensorId);

        if (sensor != null)
        {
            triggerDto.SetKnownValue("channel", (int)sensor.Channel);
            triggerDto.SetKnownValue("lowvalue", sensor.LowValue);
            triggerDto.SetKnownValue("highvalue", sensor.HighValue);
            triggerDto.SetKnownValue("engineeringunits", sensor.EngineeringUnits);
            triggerDto.SetKnownValue("sensorname", sensor.Name);
        }
    }

    private static readonly string[] CalculateSensor_KEYS = { "channel", "lowvalue", "highValue", "engineeringunits", "sensorname" };
    private static readonly string[] CalculateRtu_KEYS = { "rtuid", "phonenumber", "manufacturerid", "clientid" };

    /// <summary>
    ///     Substitute any {...} values in s for their values, and return the substituted string.
    /// </summary>
    /// <param name="templateSnippet"></param>
    /// <returns></returns>
    public string Substitute(BreachedTriggerDto trigger, string templateSnippet)
    {
        if (string.IsNullOrWhiteSpace(templateSnippet))
            return templateSnippet;
        var outputBuilder = new StringBuilder();

        foreach (var chunk in SubstitutionParser.ToChunkList(templateSnippet).Chunks)
        {
            if (!chunk.IsSubstitution)
            {
                outputBuilder.Append(chunk.Text);
                continue;
            }

            var value = CalculateTemplatePlaceholderValue(trigger, chunk.Text.Replace(" ", ""));
            if (null != value)
                outputBuilder.Append(value);
        }

        return outputBuilder.ToString();
    }

    public object? CalculateTemplatePlaceholderValue(BreachedTriggerDto alarm, string fieldName)
    {
        if (!alarm.KnownValues.ContainsKey(fieldName))
            CalculateAlarmKnownValue(alarm, fieldName);
        return alarm.KnownValues[fieldName];
    }
    internal async void CalculateAlarmKnownValue(BreachedTriggerDto alarm, string key)
    {
        logger.LogDebug("Calculate value for template variable {Key}", key);
        // First try to get a calculator that does this by its name
        if (CalculateSensor_KEYS.Contains(key.ToLower()))
        {
            logger.LogDebug("Calculate value for template variable {Key} using CalculateSensor", key);
            await CalculateSensorAsync(alarm);
            return;
        }
        if (CalculateRtu_KEYS.Contains(key.ToLower()))
        {
            logger.LogDebug("Calculate value for template variable {Key} using CalculateRtu", key);
            await unitRepository.CalculateRtuAsync(alarm);
            return;
        }

        // Failing that, try the user-defined values
        if (alarm.KnownValuesSet)
        {
            logger.LogTrace("Already loaded values, set value to null");
            // If the user values are already set and nothing else has picked it up, it's unknown
            alarm.SetKnownValue(key, null);
            return;
        }

        CalculateCustomDataForSensor(alarm);

        alarm.KnownValuesSet = true;

        // If we still don't know it after picking up the user variables, it's unknown
        if (!alarm.KnowsValue(key))
            alarm.SetKnownValue(key, null);
    }

    private void CalculateCustomDataForSensor(BreachedTriggerDto alarm)
    {
        throw new NotImplementedException("Custom data calculation is not implemented yet.");
        //public bool CalculateV2CustomerDataForSensor(AlarmServerDto alarmTrigger)
        //{
        //    using var cn = sqlConnectionProvider.GetOpenConnection();
        //    using (var cmd = cn.CreateCommand())
        //    {
        //        cmd.CommandText = @"
        //                SELECT R.CustomFieldValues 
        //                FROM dbo.RTUs R 
        //                    INNER JOIN dbo.Sensors S ON R.RTUID = S.RTUID 
        //                WHERE S.SensorID = @SensorId";
        //        cmd.Parameters.AddWithValue("@SensorId", alarmTrigger.SensorId);

        //        var valuesBlob = cmd.ExecuteScalar() as string;

        //        if (valuesBlob == null)
        //        {
        //            logger.LogDebug("No field values defined");
        //            return false;
        //        }

        //        using var definitionsCms = cn.CreateCommand();
        //        definitionsCms.CommandText = @"
        //                SELECT TOP 1 ParentCompany.CustomFieldDefinitions
        //                FROM dbo.Companies ParentCompany
        //                    INNER JOIN dbo.CompaniesAndDescendants
        //                        ON ParentCompany.CompanyID = CompaniesAndDescendants.AncestorCompanyID
        //                    INNER JOIN dbo.RTUs RU ON CompaniesAndDescendants.DescendantCompanyID = RU.CompanyID 
        //                    INNER JOIN dbo.Sensors S ON RU.RTUID = S.RTUID 
        //                WHERE S.SensorID = @SensorId
        //                AND ParentCompany.CustomFieldDefinitions IS NOT NULL
        //                ORDER BY CompaniesAndDescendants.Level";
        //        definitionsCms.Parameters.AddWithValue("@SensorId", alarmTrigger.SensorId);

        //        var definitionsBlob = definitionsCms.ExecuteScalar() as string;

        //        if (definitionsBlob == null)
        //        {
        //            logger.LogDebug("No custom fields defined");
        //            return true;
        //        }


        //        try
        //        {
        //            logger.LogTrace("Loading custom field definitions from {ValuesBlob}", definitionsBlob);
        //            var parsedDefinitions = JArray.Parse(definitionsBlob);
        //            var definitions = new Dictionary<string, string>();
        //            if (parsedDefinitions != null)
        //            {
        //                definitions = parsedDefinitions!.Select(definitionJObject => new
        //                {
        //                    Id = definitionJObject.Value<string>("Id")!,
        //                    Title = definitionJObject.Value<string>("Name")
        //                }).ToDictionary(d => d.Id, d => TitleForAlarm(d.Title!));
        //            }


        //            logger.LogTrace("Loading custom field values from {ValuesBlob}", valuesBlob);
        //            var customFieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(valuesBlob)!;
        //            foreach (var keyValuePair in customFieldValues)
        //            {
        //                if (definitions.TryGetValue(keyValuePair.Key, out var title))
        //                {
        //                    logger.LogTrace("Setting custom field value {Key} to {Value}", title, keyValuePair.Value);
        //                    alarmTrigger.SetKnownValue(title, keyValuePair.Value);
        //                }
        //                else
        //                {
        //                    logger.LogTrace("Unknown custom field {@Value}", keyValuePair);
        //                }
        //            }
        //        }
        //        catch (JsonReaderException o)
        //        {
        //            logger.LogWarning(o, "Cannot decipher Custom Fields Json {Definition} and values {Values}", definitionsBlob, valuesBlob);
        //        }
        //    }

        //   return true;
    }

}
