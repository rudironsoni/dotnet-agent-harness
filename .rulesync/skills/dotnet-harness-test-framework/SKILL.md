---
name: dotnet-harness-test-framework
description: 'Comprehensive testing framework for skills'
targets: ['*']
tags: ['harness', 'testing', 'quality']
version: '0.0.1'
author: 'dotnet-harness'
license: MIT
invocable: true
---

# Skill Testing Framework

Validate skill functionality with automated tests.

## Test Structure

```text
skill/
├── SKILL.md
└── test-cases/
    ├── basic.yml
    ├── edge-cases.yml
    └── integration.yml
```

## Test Format

```yaml
name: Basic Functionality
description: Test core skill behavior

setup:
  - mkdir -p /tmp/test-project
  - cd /tmp/test-project

tests:
  - name: Should load successfully
    input: invoke skill
    expected:
      status: success
      output_contains: 'Skill loaded'

  - name: Should reject invalid input
    input: invoke skill with "invalid"
    expected:
      status: error
      error_contains: 'Invalid input'

teardown:
  - rm -rf /tmp/test-project
```

## Commands

- `dotnet-harness:test <skill>` - Run all tests
- `dotnet-harness:test <skill> --filter basic` - Filter tests
- `dotnet-harness:test --all` - Test all skills
- `dotnet-harness:test --watch` - Watch mode

## Test Types

1. **Unit Tests**: Individual skill components
2. **Integration**: Multiple skills together
3. **E2E**: Full workflow testing
4. **Regression**: Prevent known issues

## Assertions

- `status: success|error`
- `output_contains: "string"`
- `output_matches: /regex/`
- `file_exists: "path"`
- `no_errors` - Check stderr empty

## Report Format

```text
✓ skill-name
  ✓ basic functionality (12ms)
  ✓ edge cases (8ms)
  ✗ integration test (failed)
    Expected: "success"
    Got: "error: missing dependency"

Results: 2 passed, 1 failed
```

## CI Integration

```yaml
- name: Test Skills
  run: |
    dotnet-harness:test --all --format junit > results.xml

- name: Upload Results
  uses: actions/upload-artifact@v4
  with:
    name: test-results
    path: results.xml
```

## Best Practices

1. Test happy path first
2. Include error cases
3. Use realistic inputs
4. Clean up test artifacts
5. Mock external dependencies

## Troubleshooting

**Test fails**: Check skill dependencies **Timeout**: Increase timeout in test config **Flaky tests**: Use deterministic
inputs
