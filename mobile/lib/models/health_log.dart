/// A health / isolation log entry for a single animal.
class HealthLog {
  final String id;
  final String cattleId;
  final String cattleTag;
  final String diagnosis;
  final String treatmentPlan;
  final String? vetNotes;
  final DateTime? isolatedAt;
  final DateTime? lastUpdatedAt;

  const HealthLog({
    required this.id,
    required this.cattleId,
    required this.cattleTag,
    required this.diagnosis,
    required this.treatmentPlan,
    this.vetNotes,
    this.isolatedAt,
    this.lastUpdatedAt,
  });

  /// Number of days the animal has been isolated (from [isolatedAt] to now).
  int get daysIsolated {
    if (isolatedAt == null) return 0;
    return DateTime.now().difference(isolatedAt!).inDays;
  }

  /// Number of days since the log was last updated.
  int get daysSinceUpdate {
    final ref = lastUpdatedAt ?? isolatedAt;
    if (ref == null) return 0;
    return DateTime.now().difference(ref).inDays;
  }

  /// Flagged when isolated for more than 7 days without an update.
  bool get isStale => daysIsolated > 7 && daysSinceUpdate > 7;

  factory HealthLog.fromJson(Map<String, dynamic> json) {
    return HealthLog(
      id: (json['id'] ?? '').toString(),
      cattleId: (json['cattleId'] ?? '').toString(),
      cattleTag: (json['cattleTag'] ?? json['tagNumber'] ?? '').toString(),
      diagnosis: (json['diagnosis'] ?? '').toString(),
      treatmentPlan: (json['treatmentPlan'] ?? '').toString(),
      vetNotes: json['vetNotes'] as String?,
      isolatedAt: DateTime.tryParse(
        (json['isolatedAt'] ?? json['createdAt'] ?? '').toString(),
      ),
      lastUpdatedAt: DateTime.tryParse(
        (json['lastUpdatedAt'] ?? json['updatedAt'] ?? '').toString(),
      ),
    );
  }
}
