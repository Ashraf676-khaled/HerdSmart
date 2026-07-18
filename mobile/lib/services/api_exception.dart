/// Thrown by the API layer for any network / parsing / HTTP failure.
///
/// Carries a human-friendly [message] safe to show in the UI plus an
/// optional [statusCode] for callers that want to branch on it.
class ApiException implements Exception {
  final String message;
  final int? statusCode;

  const ApiException(this.message, {this.statusCode});

  factory ApiException.network() => const ApiException(
    'Cannot reach the HerdSmart server. Check that the backend is running '
    'and reachable on the configured address.',
  );

  factory ApiException.timeout() =>
      const ApiException('The request timed out. Please try again.');

  factory ApiException.badResponse(int statusCode) => ApiException(
    'The server responded with an error (HTTP $statusCode).',
    statusCode: statusCode,
  );

  factory ApiException.parse() =>
      const ApiException('Received an unexpected response from the server.');

  @override
  String toString() => message;
}
