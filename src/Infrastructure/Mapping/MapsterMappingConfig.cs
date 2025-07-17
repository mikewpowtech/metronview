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
            TypeAdapterConfig<Company, CompanyDb>.NewConfig();
            TypeAdapterConfig<CompanyDb, Company>.NewConfig();

            // Unit mappings
            TypeAdapterConfig<Unit, UnitDb>.NewConfig();
            TypeAdapterConfig<UnitDb, Unit>.NewConfig();

            // Sensor mappings
            TypeAdapterConfig<Sensor, SensorDb>.NewConfig();
            TypeAdapterConfig<SensorDb, Sensor>.NewConfig();

            // AlarmDb mappings - map triggers collection
            //TypeAdapterConfig<AlarmDb, Alarm>.NewConfig()
            //    .Map(dest => dest.Company, src => src.Company == null ? null : new Company
            //    {
            //        Id = src.Company.Id,
            //        Name = src.Company.Name,
            //        ParentCompanyId = src.Company.ParentCompanyId
            //    })
            //    .Map(dest => dest.RecipientSet, src => src.RecipientSet == null ? null : new RecipientSet
            //    {
            //        Id = src.RecipientSet.Id,
            //        CompanyId = src.RecipientSet.CompanyId,
            //        Name = src.RecipientSet.Name
            //    })
            //    // Map Triggers collection using MapWith to convert each TriggerDb to Trigger
            //    .Map(dest => dest.Triggers, src => src.Triggers == null ? new List<Trigger>() : src.Triggers.Adapt<List<Trigger>>());

            // Configure reverse mapping for Add operations
            //TypeAdapterConfig<Alarm, AlarmDb>.NewConfig()
            //    .Ignore(dest => dest.Company)
            //    .Ignore(dest => dest.RecipientSet)
            //    .Map(dest => dest.Triggers, src => src.Triggers.Select(t => new TriggerDb
            //    {
            //        Id = t.Id,
            //        AlarmId = t.AlarmId,
            //        TriggerTypeId = t.TriggerTypeId,
            //        TriggerValue = t.TriggerValue,
            //        CommunicationModeId = t.CommunicationModeId,
            //        Subject = t.Subject,
            //        Body = t.Body,
            //        MinimumSendIntervalMinutes = t.MinimumSendIntervalMinutes,
            //        IsEnabled = t.IsEnabled,
            //        // DO NOT map Alarm here to avoid circular reference
            //        Alarm = null
            //    }).ToList());

            // NB circular reference alrm->trigger-triggertype
            // Alarm → AlarmDb mapping
            TypeAdapterConfig<Alarm, AlarmDb>.NewConfig()
                // Set navigation properties to null explicitly
                .Map(dest => dest.Company, src => (CompanyDb?)null)
                .Map(dest => dest.RecipientSet, src => (RecipientSetDb?)null)
                .Map(dest => dest.Triggers, src => new List<TriggerDb>());



            // Configure Mapster to handle circular references properly
            TypeAdapterConfig<AlarmDb, Alarm>.NewConfig()
                .AfterMapping((src, dest) =>
                {
                    // Set the parent alarm reference for each trigger after mapping
                    foreach (var trigger in dest.Triggers)
                    {
                        trigger.Alarm = dest;
                    }
                });

            TypeAdapterConfig<Trigger, TriggerDb>.NewConfig()
                // Only map scalar properties, ignore navigation properties
                // Set navigation properties to null explicitly
                .Map(dest => dest.Alarm, src => (AlarmDb?)null)
                .Map(dest => dest.TriggerType, src => (TriggerTypeDb?)null)
                .Map(dest => dest.CommunicationMode, src => (CommunicationModeDb?)null);

            // TriggerDb → Trigger mapping
            TypeAdapterConfig<TriggerDb, Trigger>.NewConfig()
                .Ignore(dest => dest.Alarm); // 🔑 This breaks the circular reference

            // Trigger mappings - prevent circular references
            //TypeAdapterConfig<TriggerDb, Trigger>.NewConfig()
            //    .Map(dest => dest.Alarm, src => src.Alarm == null ? null : new Alarm
            //    {
            //        Id = src.Alarm.Id,
            //        CompanyId = src.Alarm.CompanyId,
            //        Name = src.Alarm.Name,
            //        RecipientSetId = src.Alarm.RecipientSetId,
            //        IsActive = src.Alarm.IsActive
            //    })
            //    .Map(dest => dest.TriggerType, src => src.TriggerType)
            //    .Map(dest => dest.CommunicationMode, src => src.CommunicationMode)
            //    .Map(dest => dest.TriggerTypeId, src => src.TriggerTypeId);

        }
    }
}