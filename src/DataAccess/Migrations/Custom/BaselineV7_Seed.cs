using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.Custom
{
    internal static class BaselineV7_Seed
    {
        public static void BaselineV7_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"
                                Select * from public.""ApplicationConfiguration"";
                                ");
        }

        public static void BaselineV7_DeSeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"                                                
                                DELETE FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'WebNotificationUrl';
                               
                                DELETE FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesSuccessfulSchedulerEmail';
                               
                                DELETE FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesFailureSchedulerEmail';

                                DELETE FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesMissingSchedulerEmail';
                               
                                DELETE FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'SchedulerEmailTo';
                               
                                DELETE FROM ""EmailTemplate"" WHERE ""Status"" = 'Accepted';
                              
                                DELETE FROM ""EmailTemplate"" WHERE ""Status"" = 'Failure';
                              
                                DELETE FROM ""EmailTemplate"" WHERE ""Status"" = 'Missing';
                                ");
        }
    }
}