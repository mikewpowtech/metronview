#!/bin/bash

# MetronView Application Startup Script
echo "🚀 Starting MetronView Application..."

# Function to check if API is ready
wait_for_api() {
    echo "⏳ Waiting for API to start..."
    while ! curl -s http://localhost:5202/swagger > /dev/null 2>&1; do
        echo "   API not ready yet, waiting..."
        sleep 2
    done
    echo "✅ API is ready!"
}

# Function to cleanup on exit
cleanup() {
    echo "🛑 Stopping services..."
    pkill -f "dotnet run" 2>/dev/null
    pkill -f "vite" 2>/dev/null
    echo "✅ Services stopped"
    exit 0
}

# Set up cleanup trap
trap cleanup SIGINT SIGTERM

# Step 1: Build the solution
echo "🔨 Building solution..."
cd "$(dirname "$0")"
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

# Step 2: Start API server in background
echo "🌐 Starting API server..."
cd src/Presentation.Api
dotnet run &
API_PID=$!
cd ../..

# Step 3: Wait for API to be ready
wait_for_api

# Step 4: Start React dev server
echo "⚛️  Starting React development server..."
cd src/Presentation.Web
npm run dev -- --port 3000 &
REACT_PID=$!
cd ../..

echo "🎉 Application started successfully!"
echo "   Frontend: http://localhost:3000"
echo "   API: http://localhost:5202 (redirects to Swagger)"
echo "   Swagger: http://localhost:5202/swagger"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for user to stop
wait
