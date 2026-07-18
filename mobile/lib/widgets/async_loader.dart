import 'package:flutter/material.dart';

import '../services/api_exception.dart';
import 'error_retry_view.dart';
import 'loading_view.dart';

/// Renders the three async states (loading / error / data) for a [future].
///
/// [onRetry] should recreate the future in the parent (typically via
/// `setState`) so the loader re-runs.
class AsyncLoader<T> extends StatelessWidget {
  final Future<T>? future;
  final Widget Function(BuildContext context, T data) builder;
  final VoidCallback onRetry;
  final String? loadingMessage;

  const AsyncLoader({
    super.key,
    required this.future,
    required this.builder,
    required this.onRetry,
    this.loadingMessage,
  });

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<T>(
      future: future,
      builder: (context, snapshot) {
        switch (snapshot.connectionState) {
          case ConnectionState.none:
          case ConnectionState.waiting:
          case ConnectionState.active:
            return LoadingView(message: loadingMessage);
          case ConnectionState.done:
            if (snapshot.hasError) {
              final error = snapshot.error;
              final message = error is ApiException
                  ? error.message
                  : 'Unexpected error: $error';
              return ErrorRetryView(message: message, onRetry: onRetry);
            }
            if (snapshot.hasData) {
              return builder(context, snapshot.data as T);
            }
            return ErrorRetryView(
              message: 'No data was returned.',
              onRetry: onRetry,
            );
        }
      },
    );
  }
}
