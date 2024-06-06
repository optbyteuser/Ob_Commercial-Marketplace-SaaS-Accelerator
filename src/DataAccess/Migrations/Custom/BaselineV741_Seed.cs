using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.Custom
{
    internal static class BaselineV741_Seed
    {
        public static void BaselineV741_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"                                                  
                                INSERT INTO ""ApplicationConfiguration"" (""Name"", ""Value"", ""Description"")
                                SELECT 'IsMeteredBillingEnabled', 'true', 'Enable Metered Billing Feature'
                                WHERE NOT EXISTS (
                                    SELECT * FROM ""ApplicationConfiguration""
                                    WHERE ""Name"" = 'IsMeteredBillingEnabled'
                                );
                                ");
        }

        public static void BaselineV741_DeSeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"               
                            DELETE FROM ""ApplicationConfiguration""
                            WHERE ""Name"" = 'IsMeteredBillingEnabled';
                            ");
        }
    }
}