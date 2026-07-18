import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../models/cattle.dart';
import 'status_badge.dart';

/// A single row in the herd registry list.
class CattleCard extends StatelessWidget {
  final Cattle cattle;
  final VoidCallback? onEdit;

  const CattleCard({super.key, required this.cattle, this.onEdit});

  @override
  Widget build(BuildContext context) {
    final color = cattle.status.color;
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Container(
            width: 46,
            height: 46,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(Icons.pets_rounded, color: color),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text(
                      'Tag ${cattle.tagNumber.isEmpty ? '—' : cattle.tagNumber}',
                      style: const TextStyle(
                        color: AppColors.textPrimary,
                        fontWeight: FontWeight.w700,
                        fontSize: 15,
                      ),
                    ),
                    const SizedBox(width: 8),
                    StatusBadge(label: cattle.status.label, color: color),
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  '${cattle.breed.isEmpty ? 'Unknown breed' : cattle.breed}'
                  ' · ID ${_shortId(cattle.id)}',
                  style: const TextStyle(
                    color: AppColors.textMuted,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            onPressed: onEdit,
            icon: const Icon(
              Icons.edit_outlined,
              color: AppColors.textSecondary,
              size: 20,
            ),
            tooltip: 'Edit status',
          ),
        ],
      ),
    );
  }

  String _shortId(String id) {
    if (id.length <= 8) return id.isEmpty ? '—' : id;
    return '${id.substring(0, 8)}…';
  }
}
