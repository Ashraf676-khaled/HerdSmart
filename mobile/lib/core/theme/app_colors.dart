import 'package:flutter/material.dart';

/// Modern dark palette: slate/navy backgrounds with crisp neon accents.
class AppColors {
  AppColors._();

  // Backgrounds (slate / navy).
  static const Color background = Color(0xFF0B1120); // deep navy
  static const Color surface = Color(0xFF111A2E); // slate card
  static const Color surfaceAlt = Color(0xFF16223B); // elevated slate
  static const Color border = Color(0xFF243049);

  // Text.
  static const Color textPrimary = Color(0xFFF1F5F9);
  static const Color textSecondary = Color(0xFF94A3B8);
  static const Color textMuted = Color(0xFF64748B);

  // Neon accents.
  static const Color neonGreen = Color(0xFF22E58A);
  static const Color neonAmber = Color(0xFFFFC24B);
  static const Color neonBlue = Color(0xFF38BDF8);
  static const Color neonRed = Color(0xFFFF5C7A);
  static const Color neonPurple = Color(0xFFA78BFA);

  // Semantic (health / status).
  static const Color healthy = neonGreen;
  static const Color degraded = neonAmber;
  static const Color unhealthy = neonRed;
  static const Color info = neonBlue;
}
