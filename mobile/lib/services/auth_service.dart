/// Placeholder authentication service.
///
/// Phase 1 ships the auth *layout* only. These methods intentionally do not
/// call the backend yet — they simulate latency so the UI (spinners, error
/// states) can be exercised locally. Wire the real endpoints in a later phase
/// where each TODO is marked.
class AuthService {
  const AuthService();

  Future<void> login({
    required String email,
    required String password,
  }) async {
    // TODO(phase2): POST /api/auth/login and persist the returned JWT.
    await Future<void>.delayed(const Duration(milliseconds: 900));
  }

  Future<void> sendPasswordReset({required String email}) async {
    // TODO(phase2): POST /api/auth/forgot-password.
    await Future<void>.delayed(const Duration(milliseconds: 900));
  }

  Future<void> verifyEmail({required String code}) async {
    // TODO(phase2): POST /api/auth/verify-email.
    await Future<void>.delayed(const Duration(milliseconds: 900));
  }
}
