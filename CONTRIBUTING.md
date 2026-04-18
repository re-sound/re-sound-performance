# Contributing to re_sound Performance

Thanks for your interest in contributing. This document covers how to propose tweaks, report bugs, submit code and translations.

## Code of conduct

Be respectful. No harassment, spam, promotional content, scareware claims or unverified performance numbers. Contributions that violate this will be closed without discussion.

## Types of contributions

### 1. Tweak proposals

New tweaks are the most common contribution. Every tweak must include:

- **Source**: link to the authoritative reference (Blur Busters, Intel/AMD docs, ProSettings, Microsoft Learn, peer-reviewed benchmark)
- **What it does**: 1-2 technical sentences
- **What it modifies**: exact registry path, service name, scheduled task name, file path
- **Risk level**: SAFE, MEDIUM or HIGH
- **Anti-cheat compatibility**: verified with Vanguard, FACEIT, EAC, BattlEye
- **Expected impact**: measurable FPS, latency or system impact
- **Revert procedure**: exact steps to undo

Open an issue with the label `tweak-proposal` first. Maintainers will discuss before you write code.

### 2. Bug reports

Use the Bug Report template. Include:

- Windows build (`winver`)
- GPU vendor and driver version
- CPU model
- App version
- Steps to reproduce
- Expected vs actual behavior
- Logs from `%LOCALAPPDATA%\re_sound_performance\logs\`

### 3. Pro config updates

Pro configs for CS2, Valorant and Apex Legends must cite the source (ProSettings.net, specs.gg, HLTV, VLR.gg, player social media). Include date of verification.

### 4. Translations

Supported languages target list: English (default), Spanish (Latin America and Spain), Portuguese (Brazil), Russian, Chinese (Simplified), French, German, Japanese, Korean. Add strings to `src/Resources/` following the existing key structure.

### 5. Code contributions

## Development setup

Requirements:

- Windows 11 (for testing)
- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider or VS Code with C# extension
- Git

Setup:

```
git clone https://github.com/re-sound/re-sound-performance.git
cd re-sound-performance/src
dotnet restore
dotnet build
dotnet run
```

## Code style

Hard rules, enforced by CI:

- No comments in source code. Names must be self-descriptive.
- No emojis anywhere in source code, including strings, logs, UI text.
- Use verb-noun naming for methods. `ApplyTweak`, `RevertTweak`, `LoadProfile`.
- Use PascalCase for types and methods. camelCase for local variables and parameters.
- Nullable reference types enabled. No unnecessary null-forgiving operators.
- No unused using statements.
- Format with `dotnet format` before every commit.

Comments are only allowed in markdown files under `docs/` and in XML documentation for public API types.

## Commit messages

Use conventional commits:

- `feat(tweaks): add disable Copilot registry tweak`
- `fix(ui): correct tooltip position on narrow displays`
- `docs(readme): update installation steps`
- `test(engine): add apply/revert roundtrip test`
- `refactor(backup): extract registry backup strategy`

## Pull requests

1. Fork the repo
2. Create a topic branch from `main`
3. Make your changes with tests
4. Ensure `dotnet test` passes and `dotnet format --verify-no-changes` exits clean
5. Open a PR against `main` with a clear description
6. Link the related issue number

A maintainer will review. CI runs tests, build, linter and format check. All checks must pass.

## Security

Do not open public issues for vulnerabilities. See [SECURITY.md](SECURITY.md).

## License

By contributing, you agree your contributions are licensed under the Apache License 2.0. You represent that you have the right to submit your contributions.
