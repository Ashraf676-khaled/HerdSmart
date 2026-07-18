/// A single milk-production log entry.
class MilkLog {
  final String id;
  final String cattleId;
  final String cattleTag;
  final double amountInLiters;
  final String shift; // Morning / Afternoon / Evening
  final DateTime? loggedAt;

  const MilkLog({
    required this.id,
    required this.cattleId,
    required this.cattleTag,
    required this.amountInLiters,
    required this.shift,
    this.loggedAt,
  });

  factory MilkLog.fromJson(Map<String, dynamic> json) {
    return MilkLog(
      id: (json['id'] ?? '').toString(),
      cattleId: (json['cattleId'] ?? '').toString(),
      cattleTag: (json['cattleTag'] ?? json['tagNumber'] ?? '').toString(),
      amountInLiters:
          (json['amountInLiters'] as num?)?.toDouble() ??
          (json['amount'] as num?)?.toDouble() ??
          0,
      shift: (json['shift'] ?? '').toString(),
      loggedAt: DateTime.tryParse((json['loggedAt'] ?? '').toString()),
    );
  }
}

/// Summary block for the milk-production tab (consistency + missed logging).
class MilkLogSummary {
  final int loggedTodayCount;
  final int totalActiveCattle;
  final double consistencyPercent;

  /// Active cattle that missed logging within the last 2 days.
  final List<Cattle2Days> missedRecently;

  final List<MilkLog> latest;

  const MilkLogSummary({
    required this.loggedTodayCount,
    required this.totalActiveCattle,
    required this.consistencyPercent,
    required this.missedRecently,
    required this.latest,
  });

  factory MilkLogSummary.fromJson(Map<String, dynamic> json) {
    return MilkLogSummary(
      loggedTodayCount: (json['loggedTodayCount'] as num?)?.toInt() ?? 0,
      totalActiveCattle: (json['totalActiveCattle'] as num?)?.toInt() ?? 0,
      consistencyPercent:
          (json['consistencyPercent'] as num?)?.toDouble() ?? 0,
      missedRecently:
          ((json['missedRecently'] ?? json['missed']) as List? ?? [])
              .whereType<Map<String, dynamic>>()
              .map(Cattle2Days.fromJson)
              .toList(),
      latest: ((json['latest'] ?? json['logs']) as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map(MilkLog.fromJson)
          .toList(),
    );
  }
}

/// Lightweight cattle reference with a "days since last log" figure.
class Cattle2Days {
  final String cattleId;
  final String tagNumber;
  final int daysSinceLastLog;

  const Cattle2Days({
    required this.cattleId,
    required this.tagNumber,
    required this.daysSinceLastLog,
  });

  factory Cattle2Days.fromJson(Map<String, dynamic> json) {
    return Cattle2Days(
      cattleId: (json['cattleId'] ?? json['id'] ?? '').toString(),
      tagNumber: (json['tagNumber'] ?? json['tag'] ?? '').toString(),
      daysSinceLastLog: (json['daysSinceLastLog'] as num?)?.toInt() ?? 0,
    );
  }
}
