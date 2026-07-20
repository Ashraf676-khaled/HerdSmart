import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../models/health_report.dart';
import '../models/health_status.dart';

/// The hero card on the home dashboard summarising overall system health.
class MasterStatusCard extends StatelessWidget {
  final HealthReport report;
  final VoidCallback onRefresh;
  final bool isRefreshing;

  const MasterStatusCard({
    super.key,
    required this.report,
    required this.onRefresh,
    this.isRefreshing = false,
  });

  @override
  Widget build(BuildContext context) {
    final color = report.status.color;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(20),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            color.withValues(alpha: 0.22),
            AppColors.surface,
          ],
        ),
        border: Border.all(color: color.withValues(alpha: 0.45)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Text(
                'System Health',
                style: TextStyle(
                  color: AppColors.textSecondary,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const Spacer(),
              _RefreshButton(onRefresh: onRefresh, isRefreshing: isRefreshing),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.18),
                  shape: BoxShape.circle,
                ),
                child: Icon(report.status.icon, color: color, size: 30),
              ),
              const SizedBox(width: 16),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    report.status.label,
                    style: TextStyle(
                      color: color,
                      fontSize: 26,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    _durationLabel(report.totalDurationMs),
                    style: const TextStyle(
                      color: AppColors.textMuted,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _durationLabel(double? ms) {
    if (ms == null) return 'Total duration: —';
    return 'Total duration: ${ms.toStringAsFixed(0)} ms';
  }
}

class _RefreshButton extends StatelessWidget {
  final VoidCallback onRefresh;
  final bool isRefreshing;

  const _RefreshButton({required this.onRefresh, required this.isRefreshing});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.surfaceAlt,
      borderRadius: BorderRadius.circular(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: isRefreshing ? null : onRefresh,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              isRefreshing
                  ? const SizedBox(
                      width: 14,
                      height: 14,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: AppColors.neonBlue,
                      ),
                    )
                  : const Icon(
                      Icons.refresh_rounded,
                      size: 16,
                      color: AppColors.neonBlue,
                    ),
              const SizedBox(width: 6),
              const Text(
                'Refresh',
                style: TextStyle(
                  color: AppColors.neonBlue,
                  fontWeight: FontWeight.w700,
                  fontSize: 13,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
