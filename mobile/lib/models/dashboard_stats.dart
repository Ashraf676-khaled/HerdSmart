/// Quick farm stats displayed as summary tiles on the home dashboard.
class DashboardStats {
  final int totalCattle;
  final int activeAlerts;
  final double todaysMilkLiters;

  const DashboardStats({
    required this.totalCattle,
    required this.activeAlerts,
    required this.todaysMilkLiters,
  });

  factory DashboardStats.fromJson(Map<String, dynamic> json) {
    return DashboardStats(
      totalCattle: (json['totalCattle'] as num?)?.toInt() ?? 0,
      activeAlerts: (json['activeAlerts'] as num?)?.toInt() ?? 0,
      todaysMilkLiters:
          (json['todaysMilkLiters'] as num?)?.toDouble() ??
          (json['todaysMilkOutput'] as num?)?.toDouble() ??
          0,
    );
  }

  static const empty = DashboardStats(
    totalCattle: 0,
    activeAlerts: 0,
    todaysMilkLiters: 0,
  );
}
