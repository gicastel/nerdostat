# Dev Container Setup for Nerdostat React App

This directory contains the Dev Container configuration for developing the Nerdostat React application in a containerized environment.

## Prerequisites

- Docker Desktop installed and running
- Visual Studio Code with the "Dev Containers" extension installed

## Getting Started

### Option 1: Using VS Code Dev Containers

1. Open VS Code in the `webapp_react` folder
2. Press `F1` and select **Dev Containers: Reopen in Container**
3. Wait for the container to build and start (first time will take a few minutes)
4. The container will automatically run `npm install`
5. Start the dev server:
   ```bash
   npm run dev
   ```
6. Access the app at `http://localhost:3000`

### Option 2: Using Docker Compose

```bash
# Build and start the container
docker-compose -f .devcontainer/docker-compose.yml up -d

# Enter the container
docker-compose -f .devcontainer/docker-compose.yml exec react-app bash

# Install dependencies (if not already done)
npm install

# Start the dev server
npm run dev
```

## What's Included

### Base Image
- Node.js 20 (LTS)
- npm, yarn, pnpm
- Git
- Common build tools

### VS Code Extensions
- ESLint - JavaScript/TypeScript linting
- Prettier - Code formatting
- Tailwind CSS IntelliSense - CSS class suggestions
- ES7+ React/Redux snippets - React code snippets
- TypeScript language features

### Editor Settings
- Auto-format on save with Prettier
- ESLint auto-fix on save
- TypeScript support

### Port Forwarding
- Port 3000 - React dev server (Vite)

## Environment Variables

The dev container includes default environment variables. To override:

1. Create a `.env.local` file in the `webapp_react` folder:
   ```env
   VITE_API_BASE_URL=http://localhost:7071
   VITE_API_KEY=your_function_key_here
   ```

2. Rebuild the container to pick up changes

## Running with the API

If you need to run the Azure Functions API alongside the React app:

### Option 1: Run API on Host Machine
The dev container uses `network_mode: host` to access services running on your host machine.

1. On your host machine, start the API:
   ```bash
   cd api/bin/Debug/net8.0
   func host start
   ```

2. In the dev container, the React app can access it at `http://localhost:7071`

### Option 2: Multi-Container Setup (Advanced)
Uncomment the `api` service in `.devcontainer/docker-compose.yml` to run both services in containers.

## Troubleshooting

### Container won't start
- Ensure Docker Desktop is running
- Check Docker has sufficient resources (4GB+ RAM recommended)
- Try rebuilding: `Dev Containers: Rebuild Container`

### npm install fails
- Delete `node_modules` and `package-lock.json`
- Rebuild the container
- Run `npm install` manually inside the container

### Port 3000 already in use
- Stop other processes using port 3000
- Or modify the port in `devcontainer.json` and `vite.config.ts`

### Can't access API at localhost:7071
- Ensure the API is running on the host machine
- Verify `network_mode: host` is set in `docker-compose.yml`
- On Windows/Mac, you may need to use `host.docker.internal` instead of `localhost`

## Customization

### Change Node.js Version
Edit `.devcontainer/Dockerfile`:
```dockerfile
ARG VARIANT="18-bullseye"  # Change to 16, 18, or 20
```

### Add VS Code Extensions
Edit `.devcontainer/devcontainer.json`:
```json
"extensions": [
  "dbaeumer.vscode-eslint",
  "your-extension-id"
]
```

### Install Additional Tools
Edit `.devcontainer/Dockerfile`:
```dockerfile
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends git curl
```

## Benefits of Dev Containers

✅ **Consistent environment** - Same Node.js version across all developers  
✅ **Isolated dependencies** - No conflicts with host machine  
✅ **Easy onboarding** - New developers can start in minutes  
✅ **VS Code integration** - Full IDE support with extensions  
✅ **Reproducible builds** - Build once, run anywhere  

## Additional Resources

- [VS Code Dev Containers Documentation](https://code.visualstudio.com/docs/devcontainers/containers)
- [Dev Container Specification](https://containers.dev/)
- [Node.js Dev Container Images](https://github.com/devcontainers/images/tree/main/src/typescript-node)
