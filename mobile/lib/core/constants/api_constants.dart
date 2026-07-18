/// Central place for all backend network configuration.
///
/// The base URL is intentionally a single global constant so it can be
/// adjusted in exactly one spot when the backend host/port changes.
class ApiConstants {
  ApiConstants._();

  /// Root URL of the HerdSmart backend API.
  ///
  /// NOTE: keep the scheme (http/https) and port here. Cleartext (http)
  /// traffic is enabled in the Android manifest for local testing.
  static const String baseUrl = 'http://192.168.100.8:5168';

  // ---- Endpoints -----------------------------------------------------------

  /// ASP.NET health-checks endpoint (infrastructure + business checks).
  static const String health = '/health';

  /// Dashboard quick stats (total cattle, active alerts, today's milk).
  static const String dashboardStats = '/api/dashboard/summary';

  /// Herd registry.
  static const String cattle = '/api/cattle';

  /// Milk production logs.
  static const String milkLogs = '/api/milk-production';

  /// Health / isolation logs.
  static const String healthLogs = '/api/health-logs';

  /// Vaccination schedule.
  static const String vaccinations = '/api/vaccination-schedule';

  // ---- Timeouts ------------------------------------------------------------

  static const Duration requestTimeout = Duration(seconds: 15);
}
