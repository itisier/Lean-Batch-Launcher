using System.Linq;

namespace Panoptes.Model
{
    public static class ResultUpdater
    {
        public static void Merge(QCResult target, QCResult source)
        {
            MergeCharts(target, source);
            MergeOrders(target, source);
            MergeStatistics(target, source);

            UpdateProfitLoss(target, source);

            UpdateRollingWindow(target, source);

            UpdateResultType(target, source);
        }

        private static void UpdateResultType(QCResult target, QCResult source)
        {
            if (target.ResultType == ResultType.Backtest && source.ResultType != ResultType.Backtest)
            {
                target.ResultType = source.ResultType;
            }
        }

        private static void UpdateRollingWindow(QCResult target, QCResult source)
        {
            foreach (var x in source.RollingWindow)
            {
                target.RollingWindow[x.Key] = x.Value;
            }
        }

        private static void MergeCharts(QCResult target, QCResult source)
        {
            // Update charts
            foreach (var sourceChart in source.Charts)
            {
                // Check whether the chart is already known
                if (target.Charts.ContainsKey(sourceChart.Key))
                {
                    var targetChart = target.Charts[sourceChart.Key];

                    // Existing chart. Check whether series are known
                    foreach (var sourceSeries in sourceChart.Value.Series)
                    {
                        if (targetChart.Series.ContainsKey(sourceSeries.Key))
                        {
                            // Series is already known. Update it with new values
                            var targetSeries = targetChart.Series[sourceSeries.Key];
                            targetSeries.Values.AddRange(sourceSeries.Value.Values.Except(targetSeries.Values));
                        }
                        else
                        {
                            // This is a new series. Add it.
                            target.Charts[sourceChart.Key].Series.Add(sourceSeries.Key, sourceSeries.Value);
                        }
                    }
                }
                else
                {
                    // New chart. Add it recursively.
                    target.Charts.Add(sourceChart.Key, sourceChart.Value);
                }
            }
        }

        private static void MergeOrders(QCResult target, QCResult source)
        {
            foreach (var order in source.Orders)
            {
                if (target.Orders.ContainsKey(order.Key))
                {
                    // need to update instead?
                    target.Orders.Remove(order.Key);
                }
                target.Orders.Add(order.Key, order.Value);
            }
        }

        private static void MergeStatistics(QCResult target, QCResult source)
        {
            foreach (var x in source.Statistics)
            {
                target.Statistics[x.Key] = x.Value;
            }

            foreach (var x in source.RuntimeStatistics)
            {
                target.RuntimeStatistics[x.Key] = x.Value;
            }
        }

        private static void UpdateProfitLoss(QCResult target, QCResult source)
        {
            foreach (var x in source.ProfitLoss)
            {
                target.ProfitLoss[x.Key] = x.Value;
            }
        }
    }
}
