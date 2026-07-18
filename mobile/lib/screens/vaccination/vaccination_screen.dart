import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../core/theme/app_colors.dart';
import '../../models/vaccination.dart';
import '../../services/api_client.dart';
import '../../services/vaccination_service.dart';
import '../../widgets/alert_banner.dart';
import '../../widgets/app_card.dart';
import '../../widgets/async_loader.dart';
import '../../widgets/empty_view.dart';
import '../../widgets/section_header.dart';
import '../../widgets/status_badge.dart';

/// Vaccination schedule tracker with a long-overdue alert banner.
class VaccinationScreen extends StatefulWidget {
  final ApiClient client;

  const VaccinationScreen({super.key, required this.client});

  @override
  State<VaccinationScreen> createState() => _VaccinationScreenState();
}

class _VaccinationScreenState extends State<VaccinationScreen> {
  late final VaccinationService _service = VaccinationService(widget.client);
  late Future<List<Vaccination>> _future;

  @override
  void initState() {
    super.initState();
    _future = _service.fetchSchedule();
  }

  void _reload() {
    setState(() => _future = _service.fetchSchedule());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Vaccination Schedule')),
      body: AsyncLoader<List<Vaccination>>(
        future: _future,
        onRetry: _reload,
        loadingMessage: 'Loading schedule…',
        builder: (context, all) {
          final longOverdue = all.where((v) => v.isLongOverdue).toList();
          final pending = all
              .where((v) => v.status != VaccinationStatus.completed)
              .toList()
            ..sort(_byScheduled);
          final completed = all
              .where((v) => v.status == VaccinationStatus.completed)
              .toList();

          return RefreshIndicator(
            color: AppColors.neonBlue,
            backgroundColor: AppColors.surface,
            onRefresh: () async => _reload(),
            child: ListView(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
              children: [
                if (longOverdue.isNotEmpty) ...[
                  AlertBanner(
                    title: 'Long overdue vaccinations',
                    message:
                        '${longOverdue.length} vaccination(s) are overdue by '
                        'more than 30 days. Treat affected cattle urgently.',
                  ),
                  const SizedBox(height: 20),
                ],
                if (all.isEmpty)
                  const AppCard(
                    child: EmptyView(
                      icon: Icons.vaccines_outlined,
                      message: 'No vaccinations scheduled yet.',
                    ),
                  ),
                if (pending.isNotEmpty) ...[
                  SectionHeader(title: 'Pending (${pending.length})'),
                  ...pending.map(
                    (v) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: _VaccinationTile(vaccination: v),
                    ),
                  ),
                  const SizedBox(height: 12),
                ],
                if (completed.isNotEmpty) ...[
                  SectionHeader(title: 'Completed (${completed.length})'),
                  ...completed.map(
                    (v) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: _VaccinationTile(vaccination: v),
                    ),
                  ),
                ],
              ],
            ),
          );
        },
      ),
    );
  }

  int _byScheduled(Vaccination a, Vaccination b) {
    final da = a.scheduledDate;
    final db = b.scheduledDate;
    if (da == null && db == null) return 0;
    if (da == null) return 1;
    if (db == null) return -1;
    return da.compareTo(db);
  }
}

class _VaccinationTile extends StatelessWidget {
  final Vaccination vaccination;

  const _VaccinationTile({required this.vaccination});

  @override
  Widget build(BuildContext context) {
    final scheduled = vaccination.scheduledDate != null
        ? DateFormat('MMM d, yyyy').format(vaccination.scheduledDate!)
        : '—';
    return AppCard(
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: vaccination.status.color.withValues(alpha: 0.14),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(
              Icons.vaccines_rounded,
              color: vaccination.status.color,
              size: 20,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  vaccination.vaccineName.isEmpty
                      ? 'Vaccine'
                      : vaccination.vaccineName,
                  style: const TextStyle(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  'Tag ${vaccination.cattleTag.isEmpty ? '—' : vaccination.cattleTag}'
                  ' · $scheduled',
                  style: const TextStyle(
                    color: AppColors.textMuted,
                    fontSize: 12,
                  ),
                ),
                if (vaccination.daysOverdue > 0) ...[
                  const SizedBox(height: 4),
                  Text(
                    '${vaccination.daysOverdue} days overdue',
                    style: const TextStyle(
                      color: AppColors.neonRed,
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ],
            ),
          ),
          StatusBadge(
            label: vaccination.isLongOverdue
                ? 'Long Overdue'
                : vaccination.status.label,
            color: vaccination.isLongOverdue
                ? AppColors.neonRed
                : vaccination.status.color,
          ),
        ],
      ),
    );
  }
}
