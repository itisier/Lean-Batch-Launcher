using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panoptes.Model
{
    public interface IResultSerializer
    {
        QCResult Deserialize(string pathToResult);

        Task<QCResult> DeserializeAsync(string pathToResult, CancellationToken cancellationToken);

        string Serialize(QCResult result);

        Task<string> SerializeAsync(QCResult result, CancellationToken cancellationToken);

        IAsyncEnumerable<(DateTime, string)> GetBacktestLogs(string pathToResult, CancellationToken cancellationToken);
    }
}
