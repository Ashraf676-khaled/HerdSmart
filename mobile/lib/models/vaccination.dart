import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

/// Mirrors the backend `VaccinationStatus` enum.
enum VaccinationStatus { pending, completed, overdue, unknown }

extension VaccinationStatusX on VaccinationStatus {
  static VaccinationStatus fromString(dynamic raw) {
    switch (raw?.toString().trim().toLowerCase()) {
      case 'pending':
      case '0':
        return VaccinationStatus.pending;
      case 'completed':
      case '1':
        return VaccinationStatus.completed;
      case 'overdue':
      case '2':
        return VaccinationStatus.overdue;
      default:
        return VaccinationStatus.unknown;
    }
  }

  String get label {
    final name = toString().split('.').last;
    return name[0].toUpperCase() + name.substring(1);
  }

  Color get color {
    switch (this) {
      case VaccinationStatus.completed:
        return AppColors.neonGreen;
      case VaccinationStatus.pending:
        return AppColors.neonBlue;
      case VaccinationStatus.overdue:
        return AppColors.neonRed;
      case VaccinationStatus.unknown:
        return AppColors.textMuted;
    }
  }
}

class Vaccination {
  final String id;
  final String cattleId;
  final String cattleTag;
  final String vaccineName;
  final VaccinationStatus status;
  final DateTime? scheduledDate;
  final DateTime? administeredDate;

  const Vaccination({
    required this.id,
    required this.cattleId,
    required this.cattleTag,
    required this.vaccineName,
    required this.status,
    this.scheduledDate,
    this.administeredDate,
  });

  /// Days overdue relative to the scheduled date (0 if not overdue).
  int get daysOverdue {
    if (status == VaccinationStatus.completed || scheduledDate == null) {
      return 0;
    }
    final diff = DateTime.now().difference(scheduledDate!).inDays;
    return diff > 0 ? diff : 0;
  }

  /// "Long overdue" == overdue for more than 30 days.
  bool get isLongOverdue => daysOverdue > 30;

  factory Vaccination.fromJson(Map<String, dynamic> json) {
    return Vaccination(
      id: (json['id'] ?? '').toString(),
      cattleId: (json['cattleId'] ?? '').toString(),
      cattleTag: (json['cattleTag'] ?? json['tagNumber'] ?? '').toString(),
      vaccineName:
          (json['vaccineName'] ?? json['vaccine'] ?? json['name'] ?? '')
              .toString(),
      status: VaccinationStatusX.fromString(json['status']),
      scheduledDate: DateTime.tryParse(
        (json['scheduledDate'] ?? '').toString(),
      ),
      administeredDate: DateTime.tryParse(
        (json['administeredDate'] ?? '').toString(),
      ),
    );
  }
}
