# Dockerfile for dotnet-agent-harness toolkit
# Multi-stage build for optimal size

FROM node:20-alpine AS builder

# Install build dependencies
RUN apk add --no-cache git

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production

# Production stage
FROM node:20-alpine AS production

# Install runtime dependencies
RUN apk add --no-cache \
    git \
    bash \
    jq \
    curl

# Create non-root user
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001

# Set working directory
WORKDIR /app

# Copy dependencies from builder
COPY --from=builder --chown=nodejs:nodejs /app/node_modules ./node_modules
COPY --chown=nodejs:nodejs . .

# Switch to non-root user
USER nodejs

# Expose port for potential web server
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD node --version || exit 1

# Default command
CMD ["node", "--version"]

# Labels
LABEL org.opencontainers.image.title="dotnet-agent-harness"
LABEL org.opencontainers.image.description="Comprehensive .NET development guidance toolkit"
LABEL org.opencontainers.image.source="https://github.com/rudironsoni/dotnet-agent-harness"
LABEL org.opencontainers.image.licenses="MIT"
