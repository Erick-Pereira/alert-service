#!/bin/sh
set -e

host=${REDIS_HOST:-redis}
port=${REDIS_PORT:-6379}
max_attempts=30
attempt=1

until nc -z "$host" "$port" 2>/dev/null; do
  echo "Attempt $attempt/$max_attempts: Waiting for Redis at $host:$port..."
  attempt=$((attempt + 1))
  if [ $attempt -gt $max_attempts ]; then
    echo "ERROR: Redis did not become available after $max_attempts attempts"
    exit 1
  fi
  sleep 2
done

echo "Redis is available at $host:$port"