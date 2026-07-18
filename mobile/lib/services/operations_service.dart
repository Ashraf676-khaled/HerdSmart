import '../core/constants/api_constants.dart';
import '../models/health_log.dart';
import '../models/milk_log.dart';
import 'api_client.dart';
import 'json_utils.dart';

/// Fetches milk-production and health/isolation logs for the Operations screen.
class OperationsService {
  OperationsService(this._client);

  final ApiClient _client;

  Future<MilkLogSummary> fetchMilkSummary() async {
    final data = await _client.getJson(ApiConstants.milkLogs);
    if (data is Map<String, dynamic> &&
        (data.containsKey('consistencyPercent') ||
            data.containsKey('missedRecently') ||
            data.containsKey('latest'))) {
      return MilkLogSummary.fromJson(data);
    }
    // Fall back to building a minimal summary from a plain list of logs.
    final logs = extractList(data).map(MilkLog.fromJson).toList();
    return MilkLogSummary(
      loggedTodayCount: logs.where(_isToday).length,
      totalActiveCattle: 0,
      consistencyPercent: 0,
      missedRecently: const [],
      latest: logs,
    );
  }

  Future<List<HealthLog>> fetchHealthLogs() async {
    final data = await _client.getJson(ApiConstants.healthLogs);
    return extractList(data).map(HealthLog.fromJson).toList();
  }

  bool _isToday(MilkLog log) {
    final d = log.loggedAt;
    if (d == null) return false;
    final now = DateTime.now();
    return d.year == now.year && d.month == now.month && d.day == now.day;
  }
}
