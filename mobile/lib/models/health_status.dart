import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

/// Overall / per-check health state.
enum HealthStatus { healthy, degraded, unhealthy, unknown }

extension HealthStatusX on HealthStatus {
  static HealthStatus fromString(String? raw) {
    switch ((raw ?? '').trim().toLowerCase()) {
      case 'healthy':
        return HealthStatus.healthy;
      case 'degraded':
        return HealthStatus.degraded;
      case 'unhealthy':
        return HealthStatus.unhealthy;
      default:
        return HealthStatus.unknown;
    }
  }

  String get label {
    switch (this) {
      case HealthStatus.healthy:
        return 'Healthy';
      case HealthStatus.degraded:
        return 'Degraded';
      case HealthStatus.unhealthy:
        return 'Unhealthy';
      case HealthStatus.unknown:
        return 'Unknown';
    }
  }

  Color get color {
    switch (this) {
      case HealthStatus.healthy:
        return AppColors.healthy;
      case HealthStatus.degraded:
        return AppColors.degraded;
      case HealthStatus.unhealthy:
        return AppColors.unhealthy;
      case HealthStatus.unknown:
        return AppColors.textMuted;
    }
  }

  IconData get icon {
    switch (this) {
      case HealthStatus.healthy:
        return Icons.check_circle_rounded;
      case HealthStatus.degraded:
        return Icons.warning_amber_rounded;
      case HealthStatus.unhealthy:
        return Icons.error_rounded;
      case HealthStatus.unknown:
        return Icons.help_outline_rounded;
    }
  }
}
