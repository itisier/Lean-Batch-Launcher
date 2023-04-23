using Panoptes.Model.Serialization.Packets;

namespace Panoptes.Model
{
    public interface IResultConverter
    {
        QCResult FromBacktestResult(BacktestResult backtestResult);
        QCResult FromLiveResult(LiveResult liveResult);

        BacktestResult ToBacktestResult(QCResult result);
        LiveResult ToLiveResult(QCResult result);
    }
}
