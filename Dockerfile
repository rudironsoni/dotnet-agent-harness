# Dockerfile for dotnet-agent-harness toolkit
# Multi-stage build for optimal size

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine AS builder

# Install build dependencies
RUN apk add --no-cache git curl bash

# Install rulesync globally
RUN curl -fsSL https://github.com/dyoshikawa/rulesync/releases/latest/download/rulesync-linux-x64 -o /usr/local/bin/rulesync && \
    chmod +x /usr/local/bin/rulesync

# Set working directory
WORKDIR /app

# Copy source
COPY . .

# Build the project
RUN dotnet build src/DotNetAgentHarness.Tools/DotNetAgentHarness.Tools.csproj -c Release

# Production stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-preview-alpine AS production

# Install runtime dependencies
RUN apk add --no-cache \
    git \
    bash \
    jq \
    curl

# Install rulesync globally (system-wide)
RUN curl -fsSL https://github.com/dyoshikawa/rulesync/releases/latest/download/rulesync-linux-x64 -o /usr/local/bin/rulesync && \
    chmod +x /usr/local/bin/rulesync

# Create non-root user
RUN addgroup -g 1001 -S dotnet && \
    adduser -S dotnet -u 1001

# Set working directory
WORKDIR /app

# Copy built artifacts from builder
COPY --from=builder /app/src/DotNetAgentHarness.Tools/bin/Release/net10.0 /app/bin
COPY --chown=dotnet:dotnet . .

# Switch to non-root user
USER dotnet

# Expose port for potential web server
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD rulesync --version || exit 1

# Default command - rulesync is installed system-wide
CMD ["rulesync", "--version"]

# Labels
LABEL org.opencontainers.image.title="dotnet-agent-harness"
LABEL org.opencontainers.image.description="Comprehensive .NET development guidance toolkit"
LABEL org.opencontainers.image.source="https://github.com/rudironsoni/dotnet-agent-harness"
LABEL org.opencontainers.image.licenses="MIT"
