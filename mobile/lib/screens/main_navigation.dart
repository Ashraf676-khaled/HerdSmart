import 'package:flutter/material.dart';

import '../services/api_client.dart';
import 'cattle/cattle_screen.dart';
import 'home/home_dashboard_screen.dart';
import 'operations/operations_screen.dart';
import 'vaccination/vaccination_screen.dart';

/// Root shell with the bottom navigation bar and the four main sections.
class MainNavigation extends StatefulWidget {
  final ApiClient client;

  const MainNavigation({super.key, required this.client});

  @override
  State<MainNavigation> createState() => _MainNavigationState();
}

class _MainNavigationState extends State<MainNavigation> {
  int _index = 0;

  late final List<Widget> _screens = [
    HomeDashboardScreen(client: widget.client),
    CattleScreen(client: widget.client),
    OperationsScreen(client: widget.client),
    VaccinationScreen(client: widget.client),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(index: _index, children: _screens),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _index,
        onTap: (i) => setState(() => _index = i),
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.dashboard_outlined),
            activeIcon: Icon(Icons.dashboard_rounded),
            label: 'Home',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.pets_outlined),
            activeIcon: Icon(Icons.pets_rounded),
            label: 'Herd',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.assignment_outlined),
            activeIcon: Icon(Icons.assignment_rounded),
            label: 'Logs',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.vaccines_outlined),
            activeIcon: Icon(Icons.vaccines_rounded),
            label: 'Vaccines',
          ),
        ],
      ),
    );
  }
}
