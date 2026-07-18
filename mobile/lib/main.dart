import 'package:flutter/material.dart';

import 'core/theme/app_theme.dart';
import 'services/api_client.dart';
import 'screens/auth/login_screen.dart';

void main() {
  runApp(const HerdSmartApp());
}

class HerdSmartApp extends StatefulWidget {
  const HerdSmartApp({super.key});

  @override
  State<HerdSmartApp> createState() => _HerdSmartAppState();
}

class _HerdSmartAppState extends State<HerdSmartApp> {
  /// Single shared HTTP client for the whole app lifecycle.
  final ApiClient _client = ApiClient();

  @override
  void dispose() {
    _client.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'HerdSmart',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.dark,
      themeMode: ThemeMode.dark,
      home: LoginScreen(client: _client),
    );
  }
}
