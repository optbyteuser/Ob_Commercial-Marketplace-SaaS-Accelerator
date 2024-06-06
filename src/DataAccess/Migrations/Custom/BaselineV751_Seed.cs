using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.Custom
{
    internal static class BaselineV751_Seed
    {
        public static void BaselineV751_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"                   
                                INSERT INTO ""ApplicationConfiguration"" (""Name"", ""Value"", ""Description"")
                                SELECT 'ValidateWebhookJwtToken', 'true', 'Validates JWT token when webhook event is received.'
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'ValidateWebhookJwtToken'
                                );
                                ");
        }

        public static void BaselineV751_DeSeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"                
                                    DELETE FROM ""ApplicationConfiguration""
                                    WHERE ""Name"" = 'ValidateWebhookJwtToken';
                                    ");
        }
    }
}