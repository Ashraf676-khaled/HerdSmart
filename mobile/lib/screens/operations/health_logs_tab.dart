import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../core/theme/app_colors.dart';
import '../../models/health_log.dart';
import '../../services/api_client.dart';
import '../../services/operations_service.dart';
import '../../widgets/alert_banner.dart';
import '../../widgets/app_card.dart';
import '../../widgets/async_loader.dart';
import '../../widgets/empty_view.dart';
import '../../widgets/section_header.dart';
import '../../widgets/status_badge.dart';

/// Tab B — currently isolated cattle with a stale-isolation warning flag.
class HealthLogsTab extends StatefulWidget {
  final ApiClient client;

  const HealthLogsTab({super.key, required this.client});

  @override
  State<HealthLogsTab> createState() => _HealthLogsTabState();
}

class _HealthLogsTabState extends State<HealthLogsTab>
    with AutomaticKeepAliveClientMixin {
  late final OperationsService _service = OperationsService(widget.client);
  late Future<List<HealthLog>> _future;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    _future = _service.fetchHealthLogs();
  }

  void _reload() {
    setState(() => _future = _service.fetchHealthLogs());
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return AsyncLoader<List<HealthLog>>(
      future: _future,
      onRetry: _reload,
      loadingMessage: 'Loading health logs…',
      builder: (context, logs) {
        final stale = logs.where((l) => l.isStale).toList();
        return RefreshIndicator(
          color: AppColors.neonBlue,
          backgroundColor: AppColors.surface,
          onRefresh: () async => _reload(),
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
            children: [
              if (stale.isNotEmpty) ...[
                AlertBanner(
                  title: 'Stale isolation',
                  message:
                      '${stale.length} animal(s) isolated for more than 7 days '
                      'without any update.',
                ),
                const SizedBox(height: 16),
              ],
              const SectionHeader(title: 'Currently Isolated'),
              if (logs.isEmpty)
                const AppCard(
                  child: EmptyView(
                    icon: Icons.health_and_safety_outlined,
                    message: 'No isolated cattle right now.',
                  ),
                )
              else
                ...logs.map(
                  (log) => Padding(
                    padding: const EdgeInsets.only(bottom: 10),
                    child: _HealthLogTile(log: log),
                  ),
                ),
            ],
          ),
        );
      },
    );
  }
}

class _HealthLogTile extends StatelessWidget {
  final HealthLog log;

  const _HealthLogTile({required this.log});

  @override
  Widget build(BuildContext context) {
    final isolatedSince = log.isolatedAt != null
        ? DateFormat('MMM d, yyyy').format(log.isolatedAt!)
        : '—';
    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'Tag ${log.cattleTag.isEmpty ? '—' : log.cattleTag}',
                style: const TextStyle(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.w700,
                  fontSize: 15,
                ),
              ),
              const Spacer(),
              if (log.isStale)
                const StatusBadge(
                  label: 'Stale',
                  color: AppColors.neonRed,
                  icon: Icons.priority_high_rounded,
                )
              else
                StatusBadge(
                  label: '${log.daysIsolated}d isolated',
                  color: AppColors.neonAmber,
                ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            log.diagnosis.isEmpty ? 'No diagnosis recorded' : log.diagnosis,
            style: const TextStyle(color: AppColors.textSecondary),
          ),
          if (log.treatmentPlan.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(
              'Plan: ${log.treatmentPlan}',
              style: const TextStyle(
                color: AppColors.textMuted,
                fontSize: 12,
              ),
            ),
          ],
          const SizedBox(height: 8),
          Row(
            children: [
              const Icon(
                Icons.calendar_today_rounded,
                size: 13,
                color: AppColors.textMuted,
              ),
              const SizedBox(width: 6),
              Text(
                'Isolated since $isolatedSince · '
                'updated ${log.daysSinceUpdate}d ago',
                style: const TextStyle(
                  color: AppColors.textMuted,
                  fontSize: 12,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
