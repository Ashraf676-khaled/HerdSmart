import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;

import '../core/constants/api_constants.dart';
import 'api_exception.dart';

/// Thin wrapper around the `http` package that centralises base-URL joining,
/// timeouts, JSON decoding and error mapping to [ApiException].
class ApiClient {
  ApiClient({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Uri _uri(String path, [Map<String, dynamic>? query]) {
    final base = Uri.parse(ApiConstants.baseUrl);
    return base.replace(
      path: path,
      queryParameters: query?.map((k, v) => MapEntry(k, v.toString())),
    );
  }

  Map<String, String> get _headers => const {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
  };

  /// GET returning a decoded JSON object.
  Future<dynamic> getJson(String path, {Map<String, dynamic>? query}) async {
    return _send(() => _client.get(_uri(path, query), headers: _headers));
  }

  /// POST with a JSON body, returning decoded JSON (or null on empty body).
  Future<dynamic> postJson(String path, {Map<String, dynamic>? body}) async {
    return _send(
      () => _client.post(
        _uri(path),
        headers: _headers,
        body: jsonEncode(body ?? {}),
      ),
    );
  }

  Future<dynamic> _send(Future<http.Response> Function() request) async {
    http.Response response;
    try {
      response = await request().timeout(ApiConstants.requestTimeout);
    } on TimeoutException {
      throw ApiException.timeout();
    } on SocketException {
      throw ApiException.network();
    } on http.ClientException {
      throw ApiException.network();
    }

    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return null;
      try {
        return jsonDecode(response.body);
      } on FormatException {
        throw ApiException.parse();
      }
    }

    // ASP.NET health endpoint can return 503 with a valid JSON body.
    if (response.statusCode == 503 && response.body.isNotEmpty) {
      try {
        return jsonDecode(response.body);
      } on FormatException {
        // fall through to error below
      }
    }

    throw ApiException.badResponse(response.statusCode);
  }

  void dispose() => _client.close();
}
