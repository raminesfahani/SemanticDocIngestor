#!/bin/bash

# Build and push script for Docker Compose deployment (Linux/Mac version)
# This script builds all images and pushes them to your container registry

set -e

# Parse arguments
REGISTRY=""
VERSION="latest"
ADDITIONAL_TAG=""

while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--registry)
    REGISTRY="$2"
            shift 2
       ;;
        -v|--version)
        VERSION="$2"
    shift 2
     ;;
        -t|--tag)
   ADDITIONAL_TAG="$2"
  shift 2
            ;;
  *)
         echo "Unknown option: $1"
            echo "Usage: $0 -r <registry> [-v <version>] [-t <additional-tag>]"
       exit 1
            ;;
    esac
done

if [ -z "$REGISTRY" ]; then
    echo "Error: Registry is required"
  echo "Usage: $0 -r <registry> [-v <version>] [-t <additional-tag>]"
    exit 1
fi

# Configuration
API_IMAGE_NAME="semanticdocingestor-api"
APPHOST_IMAGE_NAME="semanticdocingestor-apphost"

echo "=== Building Docker Images ==="

# Build API Service
echo -e "\nBuilding API Service..."
docker build -f src/apps/SemanticDocIngestor.AppHost.ApiService/Dockerfile -t ${API_IMAGE_NAME}:${VERSION} .

# Build AppHost
echo -e "\nBuilding AppHost..."
docker build -f src/apps/SemanticDocIngestor.AppHost/Dockerfile -t ${APPHOST_IMAGE_NAME}:${VERSION} .

echo -e "\n=== Tagging Images ==="

# Tag API Service
echo -e "\nTagging API Service..."
docker tag ${API_IMAGE_NAME}:${VERSION} ${REGISTRY}/${API_IMAGE_NAME}:${VERSION}
docker tag ${API_IMAGE_NAME}:${VERSION} ${REGISTRY}/${API_IMAGE_NAME}:latest

if [ -n "$ADDITIONAL_TAG" ]; then
    docker tag ${API_IMAGE_NAME}:${VERSION} ${REGISTRY}/${API_IMAGE_NAME}:${ADDITIONAL_TAG}
fi

# Tag AppHost
echo -e "\nTagging AppHost..."
docker tag ${APPHOST_IMAGE_NAME}:${VERSION} ${REGISTRY}/${APPHOST_IMAGE_NAME}:${VERSION}
docker tag ${APPHOST_IMAGE_NAME}:${VERSION} ${REGISTRY}/${APPHOST_IMAGE_NAME}:latest

if [ -n "$ADDITIONAL_TAG" ]; then
    docker tag ${APPHOST_IMAGE_NAME}:${VERSION} ${REGISTRY}/${APPHOST_IMAGE_NAME}:${ADDITIONAL_TAG}
fi

echo -e "\n=== Pushing Images to Registry ==="

# Push API Service
echo -e "\nPushing API Service..."
docker push ${REGISTRY}/${API_IMAGE_NAME}:${VERSION}
docker push ${REGISTRY}/${API_IMAGE_NAME}:latest

if [ -n "$ADDITIONAL_TAG" ]; then
    docker push ${REGISTRY}/${API_IMAGE_NAME}:${ADDITIONAL_TAG}
fi

# Push AppHost
echo -e "\nPushing AppHost..."
docker push ${REGISTRY}/${APPHOST_IMAGE_NAME}:${VERSION}
docker push ${REGISTRY}/${APPHOST_IMAGE_NAME}:latest

if [ -n "$ADDITIONAL_TAG" ]; then
    docker push ${REGISTRY}/${APPHOST_IMAGE_NAME}:${ADDITIONAL_TAG}
fi

echo -e "\n=== Build and Push Complete ==="
echo -e "\nImages pushed to:"
echo "  - ${REGISTRY}/${API_IMAGE_NAME}:${VERSION}"
echo "  - ${REGISTRY}/${API_IMAGE_NAME}:latest"
echo "  - ${REGISTRY}/${APPHOST_IMAGE_NAME}:${VERSION}"
echo "  - ${REGISTRY}/${APPHOST_IMAGE_NAME}:latest"

if [ -n "$ADDITIONAL_TAG" ]; then
    echo "  - ${REGISTRY}/${API_IMAGE_NAME}:${ADDITIONAL_TAG}"
    echo "  - ${REGISTRY}/${APPHOST_IMAGE_NAME}:${ADDITIONAL_TAG}"
fi
