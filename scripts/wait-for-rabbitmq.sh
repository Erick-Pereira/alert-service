#!/bin/sh
set -e

host=${RABBITMQ_HOST:-rabbitmq}
port=${RABBITMQ_PORT:-5672}
max_attempts=30
attempt=1

until nc -z "$host" "$port" 2>/dev/null; do
  echo "Attempt $attempt/$max_attempts: Waiting for RabbitMQ at $host:$port..."
  attempt=$((attempt + 1))
  if [ $attempt -gt $max_attempts ]; then
    echo "ERROR: RabbitMQ did not become available after $max_attempts attempts"
    exit 1
  fi
  sleep 2
done

echo "RabbitMQ is available at $host:$port"