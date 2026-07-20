import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../models/dashboard_stats.dart';
import '../../models/health_report.dart';
import '../../services/api_client.dart';
import '../../services/health_service.dart';
import '../../widgets/app_card.dart';
import '../../widgets/check_tile.dart';
import '../../widgets/error_retry_view.dart';
import '../../widgets/loading_view.dart';
import '../../widgets/master_status_card.dart';
import '../../widgets/section_header.dart';
import '../../widgets/summary_tile.dart';

/// Home / health-monitoring dashboard.
class HomeDashboardScreen extends StatefulWidget {
  final ApiClient client;

  const HomeDashboardScreen({super.key, required this.client});

  @override
  State<HomeDashboardScreen> createState() => _HomeDashboardScreenState();
}

class _HomeDashboardScreenState extends State<HomeDashboardScreen> {
  late final HealthService _service = HealthService(widget.client);

  late Future<_DashboardData> _future;
  bool _refreshing = false;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<_DashboardData> _load() async {
    // Fetch health and stats together; stats are best-effort so a missing
    // dashboard endpoint doesn't break the health view.
    final health = await _service.fetchHealth();
    DashboardStats stats;
    try {
      stats = await _service.fetchDashboardStats();
    } catch (_) {
      stats = DashboardStats.empty;
    }
    return _DashboardData(health: health, stats: stats);
  }

  void _reload() {
    setState(() => _future = _load());
  }

  Future<void> _refresh() async {
    setState(() => _refreshing = true);
    final next = _load();
    setState(() => _future = next);
    try {
      await next;
    } catch (_) {
      // Error surface handled by FutureBuilder below.
    } finally {
      if (mounted) setState(() => _refreshing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('HerdSmart')),
      body: FutureBuilder<_DashboardData>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done &&
              !snapshot.hasData) {
            return const LoadingView(message: 'Loading dashboard…');
          }
          if (snapshot.hasError && !snapshot.hasData) {
            return ErrorRetryView(
              message: snapshot.error.toString(),
              onRetry: _reload,
            );
          }
          final data = snapshot.data!;
          return RefreshIndicator(
            color: AppColors.neonBlue,
            backgroundColor: AppColors.surface,
            onRefresh: _refresh,
            child: ListView(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
              children: [
                MasterStatusCard(
                  report: data.health,
                  onRefresh: _refresh,
                  isRefreshing: _refreshing,
                ),
                const SizedBox(height: 24),
                const SectionHeader(title: 'Infrastructure & Business Checks'),
                _ChecksCard(report: data.health),
                const SizedBox(height: 24),
                const SectionHeader(title: 'Farm Snapshot'),
                _StatsGrid(stats: data.stats),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _ChecksCard extends StatelessWidget {
  final HealthReport report;

  const _ChecksCard({required this.report});

  @override
  Widget build(BuildContext context) {
    if (report.entries.isEmpty) {
      return const AppCard(
        child: Text(
          'No individual checks were reported.',
          style: TextStyle(color: AppColors.textSecondary),
        ),
      );
    }
    return AppCard(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: Column(
        children: [
          for (var i = 0; i < report.entries.length; i++) ...[
            CheckTile(entry: report.entries[i]),
            if (i != report.entries.length - 1) const Divider(height: 1),
          ],
        ],
      ),
    );
  }
}

class _StatsGrid extends StatelessWidget {
  final DashboardStats stats;

  const _StatsGrid({required this.stats});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: SummaryTile(
            icon: Icons.pets_rounded,
            value: '${stats.totalCattle}',
            label: 'Total Cattle',
            color: AppColors.neonBlue,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: SummaryTile(
            icon: Icons.notifications_active_rounded,
            value: '${stats.activeAlerts}',
            label: 'Active Alerts',
            color: AppColors.neonAmber,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: SummaryTile(
            icon: Icons.water_drop_rounded,
            value: '${stats.todaysMilkLiters.toStringAsFixed(0)} L',
            label: "Today's Milk",
            color: AppColors.neonGreen,
          ),
        ),
      ],
    );
  }
}

class _DashboardData {
  final HealthReport health;
  final DashboardStats stats;

  const _DashboardData({required this.health, required this.stats});
}
