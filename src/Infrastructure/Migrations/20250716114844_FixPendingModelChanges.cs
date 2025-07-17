using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration fixes the pending model changes by ensuring the database schema
            // matches the current ApplicationDbContext model definition
            
            // Step 1: Clean up any existing SensorId1 shadow property
            migrationBuilder.Sql(@"
                -- Remove SensorId1 shadow property if it exists
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId1')
                BEGIN
                    -- Drop foreign key constraint if it exists
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Sensors_SensorId1')
                        ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Sensors_SensorId1];
                    
                    -- Drop index if it exists
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MostRecentReadings_SensorId1')
                        DROP INDEX [IX_MostRecentReadings_SensorId1] ON [MostRecentReadings];
                    
                    -- Drop the column
                    ALTER TABLE [MostRecentReadings] DROP COLUMN [SensorId1];
                END
            ");

            // Step 2: Clean up any leftover SensorId_Temp columns from previous failed migrations
            migrationBuilder.Sql(@"
                -- Remove any leftover SensorId_Temp columns
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId_Temp')
                BEGIN
                    ALTER TABLE [MostRecentReadings] DROP COLUMN [SensorId_Temp];
                END
            ");

            // Step 3: Handle IDENTITY property removal using a simpler approach
            migrationBuilder.Sql(@"
                -- Check if MostRecentReadings table exists and has SensorId with IDENTITY
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MostRecentReadings')
                   AND EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId')
                BEGIN
                    -- Store the existing data
                    DECLARE @tempTable TABLE (
                        SensorId INT,
                        UnitId INT,
                        DateReceivedUtc DATETIME2,
                        DateRecordedUtc DATETIME2,
                        Value FLOAT
                    );
                    
                    -- Copy existing data to temp table
                    INSERT INTO @tempTable
                    SELECT SensorId, UnitId, DateReceivedUtc, DateRecordedUtc, Value
                    FROM [MostRecentReadings];
                    
                    -- Drop constraints and indexes
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Sensors_SensorId')
                        ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Sensors_SensorId];
                    
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Units_UnitId')
                        ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Units_UnitId];
                    
                    IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_MostRecentReadings')
                        ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [PK_MostRecentReadings];
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MostRecentReadings_SensorId_Unique')
                        DROP INDEX [IX_MostRecentReadings_SensorId_Unique] ON [MostRecentReadings];
                    
                    -- Drop and recreate the table with the correct schema
                    DROP TABLE [MostRecentReadings];
                    
                    CREATE TABLE [MostRecentReadings] (
                        [SensorId] INT NOT NULL,
                        [UnitId] INT NOT NULL,
                        [DateReceivedUtc] DATETIME2 NOT NULL,
                        [DateRecordedUtc] DATETIME2 NOT NULL,
                        [Value] FLOAT NULL,
                        CONSTRAINT [PK_MostRecentReadings] PRIMARY KEY ([SensorId])
                    );
                    
                    -- Create the unique index
                    CREATE UNIQUE INDEX [IX_MostRecentReadings_SensorId_Unique] ON [MostRecentReadings] ([SensorId]);
                    
                    -- Restore the data
                    INSERT INTO [MostRecentReadings] (SensorId, UnitId, DateReceivedUtc, DateRecordedUtc, Value)
                    SELECT SensorId, UnitId, DateReceivedUtc, DateRecordedUtc, Value
                    FROM @tempTable;
                END
                ELSE
                BEGIN
                    -- If table doesn't exist or SensorId doesn't have IDENTITY, just ensure constraints exist
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MostRecentReadings')
                    BEGIN
                        -- Ensure primary key exists
                        IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_MostRecentReadings')
                           AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId')
                        BEGIN
                            ALTER TABLE [MostRecentReadings] ADD CONSTRAINT [PK_MostRecentReadings] PRIMARY KEY ([SensorId]);
                        END
                        
                        -- Ensure unique index exists
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MostRecentReadings_SensorId_Unique')
                           AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId')
                        BEGIN
                            CREATE UNIQUE INDEX [IX_MostRecentReadings_SensorId_Unique] ON [MostRecentReadings] ([SensorId]);
                        END
                    END
                END
            ");

            // Step 4: Ensure foreign key relationships are properly configured
            migrationBuilder.Sql(@"
                -- Add foreign key constraint for MostRecentReadings -> Sensors
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MostRecentReadings')
                   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Sensors')
                   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Sensors_SensorId')
                BEGIN
                    ALTER TABLE [MostRecentReadings] 
                    ADD CONSTRAINT [FK_MostRecentReadings_Sensors_SensorId] 
                    FOREIGN KEY ([SensorId]) REFERENCES [Sensors]([Id]) ON DELETE NO ACTION;
                END
            ");

            migrationBuilder.Sql(@"
                -- Add foreign key constraint for MostRecentReadings -> Units
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MostRecentReadings')
                   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Units')
                   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Units_UnitId')
                BEGIN
                    ALTER TABLE [MostRecentReadings] 
                    ADD CONSTRAINT [FK_MostRecentReadings_Units_UnitId] 
                    FOREIGN KEY ([UnitId]) REFERENCES [Units]([Id]) ON DELETE NO ACTION;
                END
            ");

            // Step 5: Ensure foreign key for Readings -> Sensors exists
            migrationBuilder.Sql(@"
                -- Add foreign key constraint for Readings -> Sensors
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Readings')
                   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Sensors')
                   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Readings_Sensors_SensorId')
                BEGIN
                    ALTER TABLE [Readings] 
                    ADD CONSTRAINT [FK_Readings_Sensors_SensorId] 
                    FOREIGN KEY ([SensorId]) REFERENCES [Sensors]([Id]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the foreign key constraints
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Sensors_SensorId')
                    ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Sensors_SensorId];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Units_UnitId')
                    ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Units_UnitId];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Readings_Sensors_SensorId')
                    ALTER TABLE [Readings] DROP CONSTRAINT [FK_Readings_Sensors_SensorId];
            ");
        }
    }
}
