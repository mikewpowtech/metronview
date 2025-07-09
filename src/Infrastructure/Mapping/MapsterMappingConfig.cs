using Domain;
using Domain.Identity;
using Infrastructure.DbClasses;
using Infrastructure.Identity;
using Mapster;

namespace Infrastructure.Mapping
{
    public static class MapsterMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ApplicationUser, ApplicationUserDb>.NewConfig();
            // Add custom property mappings here if needed, e.g.:
            // .Map(dest => dest.SomeProperty, src => src.SomeOtherProperty);
            // Company mappings
            TypeAdapterConfig<Company, CompanyDb>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.ParentCompanyId, src => src.ParentCompanyId);

            TypeAdapterConfig<CompanyDb, Company>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.ParentCompanyId, src => src.ParentCompanyId);

            // Unit mappings
            TypeAdapterConfig<Unit, UnitDb>.NewConfig();
            TypeAdapterConfig<UnitDb, Unit>.NewConfig();

            // Sensor mappings
            TypeAdapterConfig<Sensor, SensorDb>.NewConfig();
            TypeAdapterConfig<SensorDb, Sensor>.NewConfig();

            // Alarm mappings - map triggers collection
            TypeAdapterConfig<AlarmDb, Alarm>.NewConfig()
                .Map(dest => dest.Company, src => src.Company == null ? null : new Company
                {
                    Id = src.Company.Id,
                    Name = src.Company.Name,
                    ParentCompanyId = src.Company.ParentCompanyId
                })
                .Map(dest => dest.RecipientSet, src => src.RecipientSet == null ? null : new RecipientSet
                {
                    Id = src.RecipientSet.Id,
                    CompanyId = src.RecipientSet.CompanyId,
                    Name = src.RecipientSet.Name
                })
                // Map Triggers collection using MapWith to convert each TriggerDb to Trigger
                .Map(dest => dest.Triggers, src => src.Triggers == null ? new List<Trigger>() : src.Triggers.Adapt<List<Trigger>>());

            TypeAdapterConfig<Alarm, AlarmDb>.NewConfig()
                // Only map scalar properties, ignore navigation properties
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CompanyId, src => src.CompanyId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.RecipientSetId, src => src.RecipientSetId)
                .Map(dest => dest.IsActive, src => src.IsActive)
                // Set navigation properties to null explicitly
                .Map(dest => dest.Company, src => (CompanyDb?)null)
                .Map(dest => dest.RecipientSet, src => (RecipientSetDb?)null)
                .Map(dest => dest.Triggers, src => new List<TriggerDb>());

            // Trigger mappings - prevent circular references
            TypeAdapterConfig<TriggerDb, Trigger>.NewConfig()
                .Map(dest => dest.Alarm, src => src.Alarm == null ? null : new Alarm
                {
                    Id = src.Alarm.Id,
                    CompanyId = src.Alarm.CompanyId,
                    Name = src.Alarm.Name,
                    RecipientSetId = src.Alarm.RecipientSetId,
                    IsActive = src.Alarm.IsActive
                    // Don't map Triggers collection to prevent circular reference
                })
                .Map(dest => dest.TriggerType, src => src.TriggerType)
                .Map(dest => dest.CommunicationMode, src => src.CommunicationMode);

            TypeAdapterConfig<Trigger, TriggerDb>.NewConfig()
                // Only map scalar properties, ignore navigation properties
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.AlarmId, src => src.AlarmId)
                .Map(dest => dest.TriggerTypeId, src => src.TriggerTypeId)
                .Map(dest => dest.TriggerValue, src => src.TriggerValue)
                .Map(dest => dest.CommunicationModeId, src => src.CommunicationModeId)
                .Map(dest => dest.Subject, src => src.Subject)
                .Map(dest => dest.Body, src => src.Body)
                .Map(dest => dest.MinimumSendIntervalMinutes, src => src.MinimumSendIntervalMinutes)
                .Map(dest => dest.IsEnabled, src => src.IsEnabled)
                // Set navigation properties to null explicitly
                .Map(dest => dest.Alarm, src => (AlarmDb?)null)
                .Map(dest => dest.TriggerType, src => (TriggerTypeDb?)null)
                .Map(dest => dest.CommunicationMode, src => (CommunicationModeDb?)null);
        }
    }
}