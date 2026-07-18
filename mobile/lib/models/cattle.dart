import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

/// Mirrors the backend `CattleStatus` enum.
enum CattleStatus {
  active,
  dry,
  pregnant,
  sick,
  isolated,
  sold,
  dead,
  unknown,
}

extension CattleStatusX on CattleStatus {
  static CattleStatus fromString(dynamic raw) {
    switch (raw?.toString().trim().toLowerCase()) {
      case 'active':
      case '0':
        return CattleStatus.active;
      case 'dry':
      case '1':
        return CattleStatus.dry;
      case 'pregnant':
      case '2':
        return CattleStatus.pregnant;
      case 'sick':
      case '3':
        return CattleStatus.sick;
      case 'isolated':
      case '4':
        return CattleStatus.isolated;
      case 'sold':
      case '5':
        return CattleStatus.sold;
      case 'dead':
      case '6':
        return CattleStatus.dead;
      default:
        return CattleStatus.unknown;
    }
  }

  String get label {
    final name = toString().split('.').last;
    return name[0].toUpperCase() + name.substring(1);
  }

  Color get color {
    switch (this) {
      case CattleStatus.active:
        return AppColors.neonGreen;
      case CattleStatus.sick:
        return AppColors.neonRed;
      case CattleStatus.isolated:
        return AppColors.neonAmber;
      case CattleStatus.pregnant:
        return AppColors.neonPurple;
      case CattleStatus.dry:
        return AppColors.neonBlue;
      case CattleStatus.sold:
      case CattleStatus.dead:
      case CattleStatus.unknown:
        return AppColors.textMuted;
    }
  }
}

class Cattle {
  final String id;
  final String tagNumber;
  final String breed;
  final String gender;
  final CattleStatus status;
  final DateTime? birthDate;

  const Cattle({
    required this.id,
    required this.tagNumber,
    required this.breed,
    required this.gender,
    required this.status,
    this.birthDate,
  });

  factory Cattle.fromJson(Map<String, dynamic> json) {
    return Cattle(
      id: (json['id'] ?? json['cattleId'] ?? '').toString(),
      tagNumber: (json['tagNumber'] ?? json['tag'] ?? '').toString(),
      breed: (json['breed'] ?? '').toString(),
      gender: (json['gender'] ?? '').toString(),
      status: CattleStatusX.fromString(json['status']),
      birthDate: _parseDate(json['birthDate']),
    );
  }
}

DateTime? _parseDate(dynamic raw) {
  if (raw == null) return null;
  return DateTime.tryParse(raw.toString());
}
