using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

namespace Infrastructure.Migrations.Helpers
{
    /// <summary>
    /// Helper class for handling IDENTITY property changes in Entity Framework migrations
    /// </summary>
    public static class IdentityPropertyMigrationHelper
    {
        /// <summary>
        /// Removes the IDENTITY property from a column
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="columnType">SQL data type of the column</param>
        /// <param name="foreignKeyConstraints">List of foreign key constraints that reference this column</param>
        public static void RemoveIdentityProperty(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            string columnType = "int",
            List<(string constraintName, string referencingTable)>? foreignKeyConstraints = null)
        {
            foreignKeyConstraints ??= new List<(string, string)>();

            var sql = new StringBuilder();

            // Step 1: Create backup
            sql.AppendLine($"-- Create backup of {tableName}");
            sql.AppendLine($"SELECT * INTO {tableName}_Backup FROM {tableName};");
            sql.AppendLine();

            // Step 2: Drop foreign key constraints
            sql.AppendLine("-- Drop foreign key constraints");
            foreach (var (constraintName, referencingTable) in foreignKeyConstraints)
            {
                sql.AppendLine($@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '{constraintName}')
                    ALTER TABLE [{referencingTable}] DROP CONSTRAINT [{constraintName}];");
            }
            sql.AppendLine();

            // Step 3: Drop primary key if this is a primary key column
            sql.AppendLine($"-- Drop primary key constraint");
            sql.AppendLine($@"
            IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_{tableName}')
                ALTER TABLE [{tableName}] DROP CONSTRAINT [PK_{tableName}];");
            sql.AppendLine();

            // Step 4: Recreate column without IDENTITY
            sql.AppendLine($"-- Recreate column without IDENTITY");
            sql.AppendLine($@"
            -- Add temporary column
            ALTER TABLE [{tableName}] ADD [{columnName}_New] {columnType} NOT NULL DEFAULT 0;
            
            -- Copy data
            UPDATE [{tableName}] SET [{columnName}_New] = [{columnName}];
            
            -- Drop old column
            ALTER TABLE [{tableName}] DROP COLUMN [{columnName}];
            
            -- Rename new column
            EXEC sp_rename '{tableName}.{columnName}_New', '{columnName}', 'COLUMN';");
            sql.AppendLine();

            // Step 5: Recreate primary key if needed
            sql.AppendLine($"-- Recreate primary key constraint");
            sql.AppendLine($"ALTER TABLE [{tableName}] ADD CONSTRAINT [PK_{tableName}] PRIMARY KEY ([{columnName}]);");
            sql.AppendLine();

            // Step 6: Recreate foreign key constraints
            sql.AppendLine("-- Recreate foreign key constraints");
            foreach (var (constraintName, referencingTable) in foreignKeyConstraints)
            {
                sql.AppendLine($@"
                ALTER TABLE [{referencingTable}] ADD CONSTRAINT [{constraintName}] 
                    FOREIGN KEY ([{GetForeignKeyColumnName(constraintName)}]) REFERENCES [{tableName}]([{columnName}]);");
            }

            migrationBuilder.Sql(sql.ToString());
        }

        /// <summary>
        /// Adds IDENTITY property to a column
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="columnType">SQL data type of the column</param>
        /// <param name="identitySeed">Starting value for IDENTITY</param>
        /// <param name="identityIncrement">Increment value for IDENTITY</param>
        /// <param name="foreignKeyConstraints">List of foreign key constraints that reference this column</param>
        public static void AddIdentityProperty(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            string columnType = "int",
            int identitySeed = 1,
            int identityIncrement = 1,
            List<(string constraintName, string referencingTable)>? foreignKeyConstraints = null)
        {
            foreignKeyConstraints ??= new List<(string, string)>();

            var sql = new StringBuilder();

            // Step 1: Create backup
            sql.AppendLine($"-- Create backup of {tableName}");
            sql.AppendLine($"SELECT * INTO {tableName}_Backup FROM {tableName};");
            sql.AppendLine();

            // Step 2: Drop foreign key constraints
            sql.AppendLine("-- Drop foreign key constraints");
            foreach (var (constraintName, referencingTable) in foreignKeyConstraints)
            {
                sql.AppendLine($@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '{constraintName}')
                    ALTER TABLE [{referencingTable}] DROP CONSTRAINT [{constraintName}];");
            }
            sql.AppendLine();

            // Step 3: Drop primary key
            sql.AppendLine($@"
            IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_{tableName}')
                ALTER TABLE [{tableName}] DROP CONSTRAINT [PK_{tableName}];");
            sql.AppendLine();

            // Step 4: Recreate column with IDENTITY
            sql.AppendLine($"-- Recreate column with IDENTITY");
            sql.AppendLine($@"
            -- Add temporary column with IDENTITY
            ALTER TABLE [{tableName}] ADD [{columnName}_New] {columnType} IDENTITY({identitySeed},{identityIncrement}) NOT NULL;
            
            -- Note: Data copying for IDENTITY columns requires special handling
            -- You may need to use SET IDENTITY_INSERT ON/OFF or manual mapping
            
            -- Drop old column
            ALTER TABLE [{tableName}] DROP COLUMN [{columnName}];
            
            -- Rename new column
            EXEC sp_rename '{tableName}.{columnName}_New', '{columnName}', 'COLUMN';");
            sql.AppendLine();

            // Step 5: Recreate primary key
            sql.AppendLine($"-- Recreate primary key constraint");
            sql.AppendLine($"ALTER TABLE [{tableName}] ADD CONSTRAINT [PK_{tableName}] PRIMARY KEY ([{columnName}]);");
            sql.AppendLine();

            // Step 6: Recreate foreign key constraints
            sql.AppendLine("-- Recreate foreign key constraints");
            foreach (var (constraintName, referencingTable) in foreignKeyConstraints)
            {
                sql.AppendLine($@"
                ALTER TABLE [{referencingTable}] ADD CONSTRAINT [{constraintName}] 
                    FOREIGN KEY ([{GetForeignKeyColumnName(constraintName)}]) REFERENCES [{tableName}]([{columnName}]);");
            }

            migrationBuilder.Sql(sql.ToString());
        }

        /// <summary>
        /// Changes the IDENTITY seed and increment values
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="newSeed">New seed value</param>
        /// <param name="newIncrement">New increment value</param>
        public static void ChangeIdentityValues(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            int newSeed,
            int newIncrement = 1)
        {
            var sql = $@"
            -- Change IDENTITY seed and increment
            DBCC CHECKIDENT ('{tableName}', RESEED, {newSeed - newIncrement});
            
            -- Note: This only affects future inserts, not the increment value
            -- To change increment, the column must be dropped and recreated";

            migrationBuilder.Sql(sql);
        }

        /// <summary>
        /// Gets the foreign key column name from constraint name
        /// This is a simple implementation - you may need to customize this
        /// </summary>
        private static string GetForeignKeyColumnName(string constraintName)
        {
            // Example: FK_Sensors_Units_UnitId -> UnitId
            var parts = constraintName.Split('_');
            return parts.Length > 3 ? parts[^1] : "Id";
        }

        /// <summary>
        /// Gets all foreign key constraints that reference a specific column
        /// </summary>
        public static List<(string constraintName, string referencingTable)> GetForeignKeyConstraints(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName)
        {
            // This would need to be implemented to query the database for actual constraints
            // For now, return common constraints for the Units table
            if (tableName == "Units" && columnName == "Id")
            {
                return new List<(string, string)>
                {
                    ("FK_Sensors_Units_UnitId", "Sensors"),
                    ("FK_Readings_Units_UnitId", "Readings"),
                    ("FK_MostRecentReadings_Units_UnitId", "MostRecentReadings"),
                    ("FK_MostRecentUnitStatuses_Units_UnitId", "MostRecentUnitStatuses"),
                    ("FK_UnitStatuses_Units_UnitId", "UnitStatuses"),
                    ("FK_ConfigurationUploads_Units_UnitId", "ConfigurationUploads")
                };
            }

            return new List<(string, string)>();
        }
    }
}