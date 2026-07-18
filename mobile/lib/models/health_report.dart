import 'health_status.dart';

/// A single infrastructure/business check inside the health report
/// (e.g. database, redis, hangfire, cattle-data, milk-production-data).
class HealthCheckEntry {
  final String name;
  final HealthStatus status;
  final double? durationMs;
  final String? description;

  const HealthCheckEntry({
    required this.name,
    required this.status,
    this.durationMs,
    this.description,
  });

  factory HealthCheckEntry.fromJson(String name, Map<String, dynamic> json) {
    return HealthCheckEntry(
      name: name,
      status: HealthStatusX.fromString(json['status'] as String?),
      durationMs: _durationToMs(json['duration'] ?? json['durationMs']),
      description: json['description'] as String?,
    );
  }
}

/// Full response of the `/health` endpoint.
///
/// Tolerant parser: supports a numeric `totalDurationMs`, a TimeSpan string
/// `totalDuration` ("00:00:00.0512"), and `entries` as either a map keyed by
/// check name or a list of `{ name, status, ... }` objects.
class HealthReport {
  final HealthStatus status;
  final double? totalDurationMs;
  final List<HealthCheckEntry> entries;

  const HealthReport({
    required this.status,
    required this.totalDurationMs,
    required this.entries,
  });

  factory HealthReport.fromJson(Map<String, dynamic> json) {
    final entries = <HealthCheckEntry>[];

    final rawEntries = json['entries'] ?? json['checks'] ?? json['results'];
    if (rawEntries is Map<String, dynamic>) {
      rawEntries.forEach((key, value) {
        if (value is Map<String, dynamic>) {
          entries.add(HealthCheckEntry.fromJson(key, value));
        }
      });
    } else if (rawEntries is List) {
      for (final item in rawEntries) {
        if (item is Map<String, dynamic>) {
          final name =
              (item['name'] ?? item['key'] ?? item['check'] ?? '').toString();
          entries.add(HealthCheckEntry.fromJson(name, item));
        }
      }
    }

    return HealthReport(
      status: HealthStatusX.fromString(json['status'] as String?),
      totalDurationMs: _durationToMs(
        json['totalDurationMs'] ?? json['totalDuration'],
      ),
      entries: entries,
    );
  }
}

/// Normalises a duration coming as milliseconds (num), a TimeSpan string
/// ("00:00:00.0512345"), or plain seconds string into milliseconds.
double? _durationToMs(dynamic raw) {
  if (raw == null) return null;
  if (raw is num) return raw.toDouble();
  if (raw is String) {
    final asNum = double.tryParse(raw);
    if (asNum != null) return asNum;
    // TimeSpan "hh:mm:ss.fffffff".
    final parts = raw.split(':');
    if (parts.length == 3) {
      final hours = int.tryParse(parts[0]) ?? 0;
      final minutes = int.tryParse(parts[1]) ?? 0;
      final seconds = double.tryParse(parts[2]) ?? 0;
      return ((hours * 3600) + (minutes * 60)) * 1000 + seconds * 1000;
    }
  }
  return null;
}
