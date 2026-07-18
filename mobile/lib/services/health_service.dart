import '../core/constants/api_constants.dart';
import '../models/dashboard_stats.dart';
import '../models/health_report.dart';
import 'api_client.dart';
import 'api_exception.dart';

/// Talks to the health-check and dashboard-summary endpoints.
class HealthService {
  HealthService(this._client);

  final ApiClient _client;

  Future<HealthReport> fetchHealth() async {
    final data = await _client.getJson(ApiConstants.health);
    if (data is Map<String, dynamic>) {
      return HealthReport.fromJson(data);
    }
    throw ApiException.parse();
  }

  Future<DashboardStats> fetchDashboardStats() async {
    final data = await _client.getJson(ApiConstants.dashboardStats);
    if (data is Map<String, dynamic>) {
      return DashboardStats.fromJson(data);
    }
    throw ApiException.parse();
  }
}
