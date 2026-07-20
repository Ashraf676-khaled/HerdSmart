import 'package:flutter/material.dart';

import '../../services/api_client.dart';
import 'health_logs_tab.dart';
import 'milk_logs_tab.dart';

/// Operations logs — Milk production (Tab A) and Health isolation (Tab B).
class OperationsScreen extends StatelessWidget {
  final ApiClient client;

  const OperationsScreen({super.key, required this.client});

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Operations Logs'),
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Milk Production'),
              Tab(text: 'Health Isolation'),
            ],
          ),
        ),
        body: TabBarView(
          physics: const NeverScrollableScrollPhysics(),
          children: [
            MilkLogsTab(client: client),
            HealthLogsTab(client: client),
          ],
        ),
      ),
    );
  }
}
