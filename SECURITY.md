# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| 1.x     | Yes       |
| 0.x     | Latest minor only during beta |

## Reporting a vulnerability

Do not open public issues for security vulnerabilities.

Use GitHub Security Advisories to report privately:

1. Go to https://github.com/re-sound/re-sound-performance/security/advisories/new
2. Provide a clear description, reproduction steps, and impact assessment
3. Wait for acknowledgment (48 hours target)
4. Coordinate on a disclosure timeline

Alternative: email the maintainer at soundx2xd@gmail.com with subject line `[SECURITY] re_sound_performance`.

## What qualifies as a vulnerability

- Privilege escalation beyond what the tool requires
- Code execution via crafted config files, profiles or update manifests
- Bypass of anti-cheat detection logic leading to unintended tweak application
- Credential leakage in logs or telemetry
- Signature verification bypass on auto-updates
- Supply chain issues in dependencies

Not in scope:

- User choosing to apply risky tweaks at their own risk
- Missing features
- Feature requests for additional anti-cheat detection beyond current coverage

## Disclosure policy

- 90-day disclosure window from acknowledgment
- Critical issues may be disclosed sooner if a fix is ready
- Credit given in release notes unless reporter requests anonymity
- No bounty program at this time

## Signed releases

Starting from v1.0, all binaries are code-signed with a SignPath Foundation certificate. Verify signatures before running:

```
Get-AuthenticodeSignature re_sound_performance.exe
```

Expected subject: `CN=re-sound, O=SignPath Foundation, ...`

Unsigned builds (pre-v1.0 alphas and betas) are clearly marked in the Releases page and should be treated as development-only.
