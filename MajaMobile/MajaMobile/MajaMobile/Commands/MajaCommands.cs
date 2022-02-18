using BiExcellence.OpenBi.Api;
using System.Threading;
using System.Threading.Tasks;

namespace MajaMobile.Commands
{
    public static class MajaCommands
    {
        public static Task AnalyzeOcrDocument(this IOpenBiSession session, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = session.CreateRequest("ANALYZE_OCR_DOCUMENT");
            request.AddParameter("ID", id);
            return request.Send(cancellationToken);
        }

        public static Task DeleteOcrDocument(this IOpenBiSession session, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = session.CreateRequest("DELETE_OCR_DOCUMENT");
            request.AddParameter("ID", id);
            return request.Send(cancellationToken);
        }
    }
}