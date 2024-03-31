FROM mcr.microsoft.com/devcontainers/base:jammy

# Install Azure Dev CLI
RUN curl -fsSL https://aka.ms/install-azd.sh | bash

# Install Azure Functions Core tools
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg \
    && mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg \
    && sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list' \
    && apt-get update && apt-get install -y azure-functions-core-tools-4
