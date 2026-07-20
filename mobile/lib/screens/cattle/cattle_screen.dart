import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../models/cattle.dart';
import '../../services/api_client.dart';
import '../../services/cattle_service.dart';
import '../../widgets/async_loader.dart';
import '../../widgets/cattle_card.dart';
import '../../widgets/empty_view.dart';

/// Herd registry — list of cattle with status badges.
class CattleScreen extends StatefulWidget {
  final ApiClient client;

  const CattleScreen({super.key, required this.client});

  @override
  State<CattleScreen> createState() => _CattleScreenState();
}

class _CattleScreenState extends State<CattleScreen> {
  late final CattleService _service = CattleService(widget.client);
  late Future<List<Cattle>> _future;

  @override
  void initState() {
    super.initState();
    _future = _service.fetchCattle();
  }

  void _reload() {
    setState(() => _future = _service.fetchCattle());
  }

  void _placeholderAction(String action) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('$action — coming in a later phase'),
        backgroundColor: AppColors.surfaceAlt,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Herd Registry')),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppColors.neonGreen,
        foregroundColor: AppColors.background,
        onPressed: () => _placeholderAction('Add new animal'),
        icon: const Icon(Icons.add),
        label: const Text('Add Animal'),
      ),
      body: AsyncLoader<List<Cattle>>(
        future: _future,
        onRetry: _reload,
        loadingMessage: 'Loading herd…',
        builder: (context, cattle) {
          if (cattle.isEmpty) {
            return const EmptyView(
              icon: Icons.pets_rounded,
              message: 'No cattle registered yet.',
            );
          }
          return RefreshIndicator(
            color: AppColors.neonBlue,
            backgroundColor: AppColors.surface,
            onRefresh: () async => _reload(),
            child: ListView.separated(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 96),
              itemCount: cattle.length,
              separatorBuilder: (_, _) => const SizedBox(height: 10),
              itemBuilder: (context, index) => CattleCard(
                cattle: cattle[index],
                onEdit: () => _placeholderAction('Edit status'),
              ),
            ),
          );
        },
      ),
    );
  }
}
