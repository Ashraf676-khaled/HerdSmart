# HerdSmart Mobile (Flutter) — Phase 1

Cross-platform (Android/iOS) client for the HerdSmart livestock management
platform. This is **Phase 1: layout, structure and codebase** — no release APK
is built yet. The goal is to review the code and test features locally before
moving to Phase 2 (release build).

## Network configuration

The backend base URL is a single global constant:

- `lib/core/constants/api_constants.dart` → `ApiConstants.baseUrl`
  (currently `http://192.168.100.8:7026`).

Cleartext (http) traffic and the internet permission are enabled for local
testing in `android/app/src/main/AndroidManifest.xml`
(`android:usesCleartextTraffic="true"` + `android.permission.INTERNET`).

## Running locally

```bash
cd mobile
flutter pub get
flutter run           # on a connected device / emulator
```

> Make sure your device/emulator can reach the backend host. When using the
> Android emulator, `10.0.2.2` maps to the host machine's `localhost`; update
> `ApiConstants.baseUrl` accordingly if the backend runs on your dev machine.

## Architecture

```
lib/
  core/
    constants/   # api_constants.dart (base URL + endpoints)
    theme/       # app_colors.dart, app_theme.dart (dark slate/navy + neon)
  models/        # health_report, health_status, cattle, milk_log,
                 # health_log, vaccination, dashboard_stats
  services/      # api_client (http), api_exception, per-domain services,
                 # auth_service (placeholder)
  widgets/       # reusable UI: async_loader, loading/error/empty views,
                 # cards, badges, banners, dashboard tiles
  screens/
    auth/        # login, forgot_password, verify_email (placeholder flow)
    home/        # health-monitoring dashboard
    cattle/      # herd registry
    operations/  # milk + health/isolation log tabs
    vaccination/ # schedule tracker
    main_navigation.dart  # bottom navigation shell
  main.dart
```

## Features (Phase 1)

- **Home / Health Monitoring** — master status card from `/health`
  (Healthy/Degraded/Unhealthy, total duration, manual refresh), per-check list
  (database, redis, hangfire, cattle-data, milk-production-data, etc.), and
  quick-stat tiles (total cattle, active alerts, today's milk).
- **Herd Registry** — cattle list with colored status badges
  (Sick = red, Isolated = amber, …) and placeholder add/edit actions.
- **Operations Logs** — Milk Production tab (consistency, latest logs, missed
  logging in the last 2 days) and Health Isolation tab (currently isolated
  cattle, stale-isolation warning after 7 days without updates).
- **Vaccination Schedule** — pending/completed tracker with a prominent banner
  for vaccinations overdue by more than 30 days.
- **Auth flow (placeholder)** — login, forgot-password and verify-email
  layouts ready for backend wiring in a later phase.

All network calls use the `http` package, show a `CircularProgressIndicator`
while loading, and render a **Retry** button on failure.

## Notes

- Auth methods in `services/auth_service.dart` are placeholders (see the
  `TODO(phase2)` markers) — they simulate latency so the UI can be exercised.
- Endpoints beyond `/health` are best-effort guesses; adjust the paths in
  `api_constants.dart` once the backend routes are finalized.
