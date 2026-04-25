#!/bin/sh
set -e

# Wait for RabbitMQ to be fully ready
rabbitmqadmin="rabbitmqadmin -H ${RABBITMQ_HOST:-rabbitmq} -P 15672 -u ${RABBITMQ_USERNAME:-admin} -p ${RABBITMQ_PASSWORD:-admin}"

echo "Waiting for RabbitMQ management plugin..."
for i in $(seq 1 30); do
  if $rabbitmqadmin list queues >/dev/null 2>&1; then
    echo "RabbitMQ management plugin is ready"
    break
  fi
  echo "Attempt $i/30: Waiting for RabbitMQ management..."
  sleep 2
done

# Declare exchanges
echo "Declaring exchanges..."
$rabbitmqadmin declare exchange name=price-monitoring-exchange type=topic durable=true auto_delete=false || true
$rabbitmqadmin declare exchange name=alert-monitoring-exchange type=fanout durable=true auto_delete=false || true
$rabbitmqadmin declare exchange name=dlx-price-monitoring type=topic durable=true auto_delete=false || true

# Declare queues
echo "Declaring queues..."
$rabbitmqadmin declare queue name=alert-service-price-analysis-queue durable=true auto_delete=false \
  arguments='{"x-dead-letter-exchange":"dlx-price-monitoring","x-message-ttl":3600000}' || true

$rabbitmqadmin declare queue name=alert-triggered-events-queue durable=true auto_delete=false || true
$rabbitmqadmin declare queue name=dlx-price-monitoring-queue durable=true auto_delete=false || true

# Bind queues
echo "Binding queues..."
$rabbitmqadmin declare binding source=price-monitoring-exchange destination=alert-service-price-analysis-queue routing_key="price.analysis.completed" || true
$rabbitmqadmin declare binding source=alert-monitoring-exchange destination=alert-triggered-events-queue || true
$rabbitmqadmin declare binding source=dlx-price-monitoring destination=dlx-price-monitoring-queue || true

echo "RabbitMQ resources created successfully"