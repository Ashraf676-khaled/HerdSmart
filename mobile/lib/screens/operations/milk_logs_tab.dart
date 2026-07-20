import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../core/theme/app_colors.dart';
import '../../models/milk_log.dart';
import '../../services/api_client.dart';
import '../../services/operations_service.dart';
import '../../widgets/alert_banner.dart';
import '../../widgets/app_card.dart';
import '../../widgets/async_loader.dart';
import '../../widgets/empty_view.dart';
import '../../widgets/section_header.dart';

/// Tab A — milk production logs, consistency, and missed-logging warnings.
class MilkLogsTab extends StatefulWidget {
  final ApiClient client;

  const MilkLogsTab({super.key, required this.client});

  @override
  State<MilkLogsTab> createState() => _MilkLogsTabState();
}

class _MilkLogsTabState extends State<MilkLogsTab>
    with AutomaticKeepAliveClientMixin {
  late final OperationsService _service = OperationsService(widget.client);
  late Future<MilkLogSummary> _future;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    _future = _service.fetchMilkSummary();
  }

  void _reload() {
    setState(() => _future = _service.fetchMilkSummary());
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return AsyncLoader<MilkLogSummary>(
      future: _future,
      onRetry: _reload,
      loadingMessage: 'Loading milk logs…',
      builder: (context, summary) {
        return RefreshIndicator(
          color: AppColors.neonBlue,
          backgroundColor: AppColors.surface,
          onRefresh: () async => _reload(),
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
            children: [
              if (summary.missedRecently.isNotEmpty) ...[
                AlertBanner(
                  title: 'Missed logging',
                  color: AppColors.neonAmber,
                  icon: Icons.report_gmailerrorred_rounded,
                  message:
                      '${summary.missedRecently.length} active cattle have no '
                      'milk log in the last 2 days.',
                ),
                const SizedBox(height: 16),
              ],
              _ConsistencyCard(summary: summary),
              const SizedBox(height: 20),
              if (summary.missedRecently.isNotEmpty) ...[
                const SectionHeader(title: 'Cattle Missing Recent Logs'),
                _MissedList(items: summary.missedRecently),
                const SizedBox(height: 20),
              ],
              const SectionHeader(title: 'Latest Logs'),
              if (summary.latest.isEmpty)
                const AppCard(
                  child: EmptyView(
                    icon: Icons.water_drop_outlined,
                    message: 'No milk logs recorded yet.',
                  ),
                )
              else
                ...summary.latest.map((log) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: _MilkLogTile(log: log),
                    )),
            ],
          ),
        );
      },
    );
  }
}

class _ConsistencyCard extends StatelessWidget {
  final MilkLogSummary summary;

  const _ConsistencyCard({required this.summary});

  @override
  Widget build(BuildContext context) {
    final pct = (summary.consistencyPercent.clamp(0, 100)) / 100.0;
    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Logging Consistency',
            style: TextStyle(
              color: AppColors.textSecondary,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                '${summary.consistencyPercent.toStringAsFixed(0)}%',
                style: const TextStyle(
                  color: AppColors.neonGreen,
                  fontSize: 30,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(width: 10),
              Padding(
                padding: const EdgeInsets.only(bottom: 6),
                child: Text(
                  '${summary.loggedTodayCount}/${summary.totalActiveCattle} '
                  'logged today',
                  style: const TextStyle(color: AppColors.textMuted),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          ClipRRect(
            borderRadius: BorderRadius.circular(999),
            child: LinearProgressIndicator(
              value: pct,
              minHeight: 8,
              backgroundColor: AppColors.surfaceAlt,
              color: AppColors.neonGreen,
            ),
          ),
        ],
      ),
    );
  }
}

class _MissedList extends StatelessWidget {
  final List<Cattle2Days> items;

  const _MissedList({required this.items});

  @override
  Widget build(BuildContext context) {
    return AppCard(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
      child: Column(
        children: [
          for (var i = 0; i < items.length; i++) ...[
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 12),
              child: Row(
                children: [
                  const Icon(
                    Icons.pets_rounded,
                    size: 18,
                    color: AppColors.neonAmber,
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      'Tag ${items[i].tagNumber}',
                      style: const TextStyle(color: AppColors.textPrimary),
                    ),
                  ),
                  Text(
                    '${items[i].daysSinceLastLog}d ago',
                    style: const TextStyle(color: AppColors.neonAmber),
                  ),
                ],
              ),
            ),
            if (i != items.length - 1) const Divider(height: 1),
          ],
        ],
      ),
    );
  }
}

class _MilkLogTile extends StatelessWidget {
  final MilkLog log;

  const _MilkLogTile({required this.log});

  @override
  Widget build(BuildContext context) {
    final when = log.loggedAt != null
        ? DateFormat('MMM d, HH:mm').format(log.loggedAt!)
        : '—';
    return AppCard(
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AppColors.neonBlue.withValues(alpha: 0.14),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(
              Icons.water_drop_rounded,
              color: AppColors.neonBlue,
              size: 18,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Tag ${log.cattleTag.isEmpty ? '—' : log.cattleTag}',
                  style: const TextStyle(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  '${log.shift.isEmpty ? '' : '${log.shift} · '}$when',
                  style: const TextStyle(
                    color: AppColors.textMuted,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          Text(
            '${log.amountInLiters.toStringAsFixed(1)} L',
            style: const TextStyle(
              color: AppColors.neonGreen,
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    );
  }
}
