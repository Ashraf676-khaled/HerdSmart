import '../core/constants/api_constants.dart';
import '../models/cattle.dart';
import 'api_client.dart';
import 'json_utils.dart';

class CattleService {
  CattleService(this._client);

  final ApiClient _client;

  Future<List<Cattle>> fetchCattle() async {
    final data = await _client.getJson(ApiConstants.cattle);
    return extractList(data).map(Cattle.fromJson).toList();
  }
}
