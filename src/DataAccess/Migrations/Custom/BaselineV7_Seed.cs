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
                               -- INSERT INTO ApplicationConfiguration (""Name"", ""Value"", ""Description"")
                               -- SELECT 'WebNotificationUrl', '', 'Setting this URL will enable pushing LandingPage/Webhook events to this external URL'
                               -- WHERE NOT EXISTS (
                               --     SELECT 1 FROM ApplicationConfiguration WHERE ""Name"" = 'WebNotificationUrl'
                              --  );

                                INSERT INTO ApplicationConfiguration (""Name"", ""Value"", ""Description"")
                                SELECT 'EnablesSuccessfulSchedulerEmail', 'False', 'This will enable sending email for successful metered usage.'
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesSuccessfulSchedulerEmail'
                                );

                                INSERT INTO ""ApplicationConfiguration"" (""Name"", ""Value"", ""Description"")
                                SELECT 'EnablesFailureSchedulerEmail', 'False', 'This will enable sending email for failure metered usage.'
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesFailureSchedulerEmail'
                                );

                                INSERT INTO ""ApplicationConfiguration"" (""Name"", ""Value"", ""Description"")
                                SELECT 'EnablesMissingSchedulerEmail', 'False', 'This will enable sending email for missing metered usage.'
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'EnablesMissingSchedulerEmail'
                                );

                                INSERT INTO ""ApplicationConfiguration"" (""Name"", ""Value"", ""Description"")
                                SELECT 'SchedulerEmailTo', '', 'Scheduler email receiver(s)'
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""ApplicationConfiguration"" WHERE ""Name"" = 'SchedulerEmailTo'
                                );
                  
                                INSERT INTO ""EmailTemplate"" (""Status"", ""Description"", ""InsertDate"", ""TemplateBody"", ""Subject"", ""IsActive"")
                                SELECT 'Accepted', 'Accepted', CURRENT_TIMESTAMP, 
                                '<html> <head> <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""/> </head> 
                                <body leftmargin=""0"" marginwidth=""0"" topmargin=""0"" marginheight=""0"" offset=""0""> 
                                <center> <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" height=""100%"" width=""100%"" id=""bodyTable""> 
                                <tr><td align=""center"" valign=""top"" id=""bodyCell""><!-- BEGIN TEMPLATE // --> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" id=""templateContainer""> 
                                <tr><td align=""center"" valign=""top""><!-- BEGIN BODY // --> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" id=""templateBody""> 
                                <tr><td valign=""top"" class=""bodyContent""><h2>Subscription ****SubscriptionName****</h2><br> 
                                <p>The Scheduled Task ****SchedulerTaskName**** was fired <b>Successfully</b></p> 
                                <p>The following section is the detail results.</p><hr/> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" id=""templateBody""> 
                                ****ResponseJson**** </table></td></tr></table></td></tr></table><!-- // END BODY --></td></tr> </table> <!-- // END TEMPLATE --> 
                                </center> </body> </html>', 'Scheduled SaaS Metered Usage Submitted Successfully!', true
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""EmailTemplate"" WHERE ""Status"" = 'Accepted'
                                );

                                INSERT INTO ""EmailTemplate"" (""Status"", ""Description"", ""InsertDate"", ""TemplateBody"", ""Subject"", ""IsActive"")
                                SELECT 'Failure', 'Failure', CURRENT_TIMESTAMP, 
                                '<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""/></head> 
                                <body leftmargin=""0"" marginwidth=""0"" topmargin=""0"" marginheight=""0"" offset=""0""> 
                                <center><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" height=""100%"" width=""100%"" id=""bodyTable""> 
                                <tr><td align=""center"" valign=""top"" id=""bodyCell""><!-- BEGIN TEMPLATE // --> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" id=""templateContainer""> 
                                <tr><td align=""center"" valign=""top""><!-- BEGIN BODY // --> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" id=""templateBody""> 
                                <tr> <td valign=""top"" class=""bodyContent""><h2 >Subscription ****SubscriptionName****</h2><br> 
                                <p>The Scheduled Task ****SchedulerTaskName**** was fired<b> but Failed to Submit Data</b></p><br> 
                                Please try again or contact technical support to troubleshoot the issue. 
                                <p>The following section is the detail results.</p><hr/> 
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" id=""templateBody""> 
                                ****ResponseJson****</table></td></tr></table></td> </tr></table><!-- // END BODY --></td></tr></table><!-- // END TEMPLATE --> 
                                </center></body></html>', 'Scheduled Task Failed to Submit Data!', true
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM ""EmailTemplate"" WHERE ""Status"" = 'Failure'
                                );

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