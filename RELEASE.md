# Release Checklist

Pre-release and post-release checklist for dotnet-agent-harness.

## Pre-Release

- [ ] All tests passing (`dotnet test`)
- [ ] Slopwatch clean (`slopwatch analyze`)
- [ ] Documentation updated
- [ ] Changelog updated
- [ ] Version bumped in:
  - [ ] `.rulesync/rules/00-overview.md`
  - [ ] `src/DotNetAgentHarness.Tools/DotNetAgentHarness.Tools.csproj`
  - [ ] `docs/index.md`
- [ ] Breaking changes documented
- [ ] Migration guide updated (if needed)
- [ ] MIGRATION.md exists and accurate
- [ ] Smoke test passed locally

## Release

1. Create annotated tag:
   ```bash
   git tag -a vX.Y.Z -m "Release vX.Y.Z: Description"
   ```

2. Push tag:
   ```bash
   git push origin vX.Y.Z
   ```

3. Wait for CI:
   - Bundle generation
   - Package build
   - Smoke tests
   - Publish to GitHub Packages
   - Publish to NuGet

## Post-Release

- [ ] GitHub release created
- [ ] Release notes published
- [ ] Documentation site updated
- [ ] Announce in discussions
- [ ] Monitor for issues (24-48 hours)
- [ ] Update downstream consumers

## Emergency Rollback

If critical issues found:

1. Delete tag: `git push origin --delete vX.Y.Z`
2. Delete GitHub release
3. Unlist from NuGet (if needed)
4. Fix issues
5. Create new tag

## Version Numbering

Follow SemVer:
- MAJOR: Breaking changes
- MINOR: New features
- PATCH: Bug fixes
