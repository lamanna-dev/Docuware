using DocuWare.Platform.ServerClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Docuware.Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var conn = await ConnectWithOrgAsync();
            var org = conn.Organizations[0];
            var fileCabinets = org.GetFileCabinetsFromFilecabinetsRelation().FileCabinet;
            foreach (var fc in fileCabinets)
            {
                Console.WriteLine("You have access to the file cabinet " + fc.Name + ".");
                var dialogInfoItems = fc.GetDialogInfosFromSearchesRelation();
                var dialog = dialogInfoItems.Dialog[0].GetDialogFromSelfRelation();
                Console.WriteLine("Dialog: " + dialog.DisplayName);
                foreach(var f in dialog.Fields)
                {
                    Console.WriteLine("     FIELD: " + f.DBFieldName);
                }
                //var query = await RunQueryAsync(dialog);
                //var t = query.Items;
                if (fc.Name == "LaManna")
                {
                    var result = ListAllDocuments(conn, fc.Id, 20);

                }
            }
            Console.ReadKey();

        }
        public static DocumentsQueryResult ListAllDocuments(ServiceConnection conn, string fileCabinetId, int? count = int.MaxValue)
        {
            DocumentsQueryResult queryResult = conn.GetFromDocumentsForDocumentsQueryResultAsync(
                fileCabinetId,
                count: count).Result;
            foreach (var document in queryResult.Items)
            {
                Console.WriteLine("Document {0} created at {1}", document.Id, document.CreatedAt);
            }
            return queryResult;
        }
        public async static Task<DocumentsQueryResult> RunQueryAsync(Dialog dialog)
        {
            var q = new DialogExpression()
            {
                Operation = DialogExpressionOperation.And,
                Condition = new List<DialogExpressionCondition>()
                {
                    DialogExpressionCondition.Create("STATUS", "APPROVED", "APPROVED" )
                },
                Count = 100,
                SortOrder = new List<SortedField>
                {
                    SortedField.Create("DWSTOREDATETIME", SortDirection.Desc)
                }
            };

            DocumentsQueryResult queryResult = await dialog.Query.PostToDialogExpressionRelationForDocumentsQueryResultAsync(q);
            foreach (var d in queryResult.Items)
            {
                Console.WriteLine("Hit {0}: \"{1}\" on {2}", d.Id, (d["SENDER"].Item as string) ?? "-", d.CreatedAt);
            }

            return queryResult;
        }
        static Uri uri = new Uri("https://invoice.ezimanager.cloud:444/DocuWare/Platform");

        static async public Task<ServiceConnection> ConnectAsync()
        {
            return await ServiceConnection.CreateAsync(uri, "admin", "admin");
        }

        static async public Task<ServiceConnection> ConnectWithOrgAsync()
        {
            return await ServiceConnection.CreateAsync(uri, "Craig Day", "Welcome123", organization: "Gap Solutions PTY LTD");
        }

        static async public Task<ServiceConnection> ConnectWithCachingAsync()
        {
            var handler = new WebRequestHandler() { CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default) };
            return await ServiceConnection.CreateAsync(uri, "admin", "admin", httpClientHandler: handler);
        }

        static async public Task<ServiceConnection> ConnectWithNTLMAsync()
        {
            return await ServiceConnection.CreateWithWindowsAuthenticationAsync(uri, "Administrator", "admin");
        }

        static async public Task<ServiceConnection> ConnectWithDefaultUserAsync()
        {
            return await ServiceConnection.CreateWithWindowsAuthenticationAsync(uri, System.Net.CredentialCache.DefaultCredentials);
        }
    }
}
