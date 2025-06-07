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
        }
    }
}