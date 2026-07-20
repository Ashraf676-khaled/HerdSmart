/// Extracts a JSON list from a variety of common API response shapes:
/// a bare array, or an envelope with `items` / `data` / `results`.
List<Map<String, dynamic>> extractList(dynamic data) {
  List raw;
  if (data is List) {
    raw = data;
  } else if (data is Map<String, dynamic>) {
    raw = (data['items'] ?? data['data'] ?? data['results'] ?? const []) as List;
  } else {
    raw = const [];
  }
  return raw.whereType<Map<String, dynamic>>().toList();
}
