# RabbitMQ for Agent Extraction

## Container Configuration

The RabbitMQ container is configured using Docker Compose in the root directory.
Configuration details:

- **Image**: rabbitmq:3.12-management (includes management UI)
- **Ports**:
  - 5672: Standard RabbitMQ port
  - 15672: Management UI port
- **Credentials**:
  - Username: admin
  - Password: adminpassword
- **Persistence**: Data is stored in a Docker volume

## Usage Instructions

### Starting RabbitMQ

```bash
# From the project root directory
docker-compose up -d rabbitmq
```

### Stopping RabbitMQ

```bash
# From the project root directory
docker-compose stop rabbitmq
```

### Accessing Management UI

Open http://localhost:15672 in your web browser
- Username: admin
- Password: adminpassword

### Verifying RabbitMQ Status

```bash
docker-compose ps
```

## Connection String for Application

Use the following connection string format in your application:

```
amqp://admin:adminpassword@localhost:5672
```
