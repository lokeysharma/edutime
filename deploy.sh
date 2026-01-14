#!/bin/bash

# Deployment script for edutime application
# Usage: ./deploy.sh [branch]

set -e

BRANCH=${1:-dev}
PROJECT_DIR=~/projects/edutime

echo "🚀 Starting deployment..."

cd $PROJECT_DIR

# Pull latest code
echo "📥 Pulling latest code from $BRANCH..."
git fetch origin
git checkout $BRANCH
git pull origin $BRANCH

# Build and restart containers
echo "🐳 Building and restarting Docker containers..."
docker compose down
docker compose up --build -d

# Wait for containers to be healthy
echo "⏳ Waiting for containers to start..."
sleep 10

# Check container status
echo "✅ Container status:"
docker ps

# Show recent logs
echo "📋 Recent API logs:"
docker logs edumgm-api --tail 20

echo ""
echo "🎉 Deployment completed!"
echo "Frontend: http://$(hostname -I | awk '{print $1}'):3000"
echo "API Swagger: http://$(hostname -I | awk '{print $1}'):5000/swagger"
