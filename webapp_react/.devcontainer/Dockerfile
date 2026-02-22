# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.245.2/containers/typescript-node/.devcontainer/base.Dockerfile

# [Choice] Node.js version: 20, 18, 16
ARG VARIANT="20-bullseye"
FROM mcr.microsoft.com/vscode/devcontainers/typescript-node:0-${VARIANT}

# Install additional OS packages if needed
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# Configure global npm packages
RUN su node -c "npm install -g npm@latest"

# Setup environment variables
ENV VITE_API_BASE_URL=https://nerdostat-api.azurewebsites.net
ENV VITE_API_KEY=jkfNpKqalhG3fq4vFb8brNE1cvgpEzgJ9rRfaEawAjcwo36dR2oY5w==

# Set working directory
WORKDIR /workspace

# Expose port for Vite dev server
EXPOSE 3000
