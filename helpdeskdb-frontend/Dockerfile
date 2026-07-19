# Build stage
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies first so the layer is cached while source code changes
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# The rewrites() proxy target in next.config.ts is resolved during `next build` and baked
# into the standalone server, so the backend URL must be known at build time. The default
# is the backend's docker-compose service name; override with --build-arg (or the compose
# build.args mapping) when the backend lives elsewhere.
ARG NEXT_PUBLIC_BACKEND_URL=http://lapikudhelpdesk-db-backend:8080
ENV NEXT_PUBLIC_BACKEND_URL=${NEXT_PUBLIC_BACKEND_URL}
ENV NODE_ENV=production
RUN npm run build

# Runtime stage — only the self-contained standalone output, no node_modules from source
FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

# The runtime only ever executes `node server.js` — remove the package managers the base
# image bundles (npm, corepack, yarn). Their vendored dependencies are what vulnerability
# scanners keep flagging, and they'd only be useful to an attacker inside the container.
RUN rm -rf /usr/local/lib/node_modules /usr/local/bin/npm /usr/local/bin/npx \
    /usr/local/bin/corepack /opt/yarn* /usr/local/bin/yarn /usr/local/bin/yarnpkg

# Run as the image's built-in non-root user (port 3000 needs no privilege)
COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static
COPY --from=build --chown=node:node /app/public ./public
USER node

EXPOSE 3000
CMD ["node", "server.js"]
