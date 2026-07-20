// Basic smoke test for the HerdSmart app shell.

import 'package:flutter_test/flutter_test.dart';

import 'package:herdsmart_mobile/main.dart';

void main() {
  testWidgets('App boots to the login screen', (WidgetTester tester) async {
    await tester.pumpWidget(const HerdSmartApp());
    await tester.pump();

    expect(find.text('HerdSmart'), findsWidgets);
    expect(find.text('Sign In'), findsOneWidget);
  });
}
