﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.Custom
{
    internal static class BaselineV6_Seed
    {
        public static void BaselineV6_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"
                                INSERT INTO ""SchedulerFrequency"" (""Frequency"")
                                SELECT 'OneTime'
                                WHERE NOT EXISTS (
                                    SELECT 1
                                    FROM ""SchedulerFrequency""
                                    WHERE ""Frequency"" = 'OneTime'
                                );

                                ");
            migrationBuilder.Sql(@$"
                                INSERT INTO ""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"" ) VALUES 
                                    ( 'EnableHourlyMeterSchedules', 'False', 'This will enable to run Hourly meter scheduled items' ),
                                    ( 'EnableDailyMeterSchedules',  'False', 'This will enable to run Daily meter scheduled items' ),
                                    ( 'EnableWeeklyMeterSchedules', 'False', 'This will enable to run Weekly meter scheduled items' ),
                                    ( 'EnableMonthlyMeterSchedules', 'False', 'This will enable to run Monthly meter scheduled items' ),
                                    ( 'EnableYearlyMeterSchedules', 'False', 'This will enable to run Yearly meter scheduled items' ),
                                    ( 'EnableOneTimeMeterSchedules', 'False', 'This will enable to run OneTime meter scheduled items' );
                                 ");
        }
    }
}