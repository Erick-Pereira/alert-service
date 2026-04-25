#!/bin/sh
set -e

echo "Waiting for RabbitMQ to become available..."
/wait-for-rabbitmq.sh

echo "Waiting for Redis to become available..."
/wait-for-redis.sh

echo "Creating RabbitMQ exchanges and queues..."
/create-rabbitmq-resources.sh

echo "Starting Alert Service..."
exec dotnet Simcag.AlertService.Api.dll