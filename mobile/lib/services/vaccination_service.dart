import '../core/constants/api_constants.dart';
import '../models/vaccination.dart';
import 'api_client.dart';
import 'json_utils.dart';

class VaccinationService {
  VaccinationService(this._client);

  final ApiClient _client;

  Future<List<Vaccination>> fetchSchedule() async {
    final data = await _client.getJson(ApiConstants.vaccinations);
    return extractList(data).map(Vaccination.fromJson).toList();
  }
}
