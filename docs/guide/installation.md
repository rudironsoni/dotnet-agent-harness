# Installation

Install dotnet-agent-harness as a local .NET tool in your repository.

## Requirements

- .NET 8.0 SDK or later
- Git repository (for bootstrap)

## Install

### 1. Create Tool Manifest

If you don't have one already:

```bash
dotnet new tool-manifest
```

### 2. Install the Tool

```bash
dotnet tool install Rudironsoni.DotNetAgentHarness
```

### 3. Restore Tools

```bash
dotnet tool restore
```

## Bootstrap

Install AI agent configurations in your repository:

```bash
dotnet agent-harness bootstrap
```

This extracts pre-built agent configurations tailored for your project.

## Verify Installation

Check everything is working:

```bash
dotnet agent-harness doctor
```

## Upgrade

Update to the latest version:

```bash
dotnet tool update Rudironsoni.DotNetAgentHarness
```

Then re-run bootstrap to update configurations:

```bash
dotnet agent-harness bootstrap
```
