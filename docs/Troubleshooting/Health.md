# Health Endpoints
Cloud environments, including Kubernetes, rely on health endpoints to monitor and maintain application performance and availability, enabling automated actions and improving system reliability.
The UPS can also run within a cloud-based environment and provide health endpoints. The types of supported health-endpoints are:

- Liveness
    - `/health/live`
    - Is the service started and theoretically functional (config/port binding/DLLs loaded)
- Readyness
    - `/health/ready`
    - Is the service capable of performing its tasks (processing messages/events/requests)
- Status
    - `/health/state`
    - Display additional information from the service

If the UPS is not running, health endpoints can help identify the issue or serve as the first step in troubleshooting. Below is an example of the /health/state endpoint from the UPS API:

```json
{
    "entries": {
        "sagaWorker": {
            "data": {
                "healthy": 1,
                "degraded": 0,
                "unhealthy": 0,
                "version": "7.4.4.92"
            },
            "status": "Healthy"
        },
        "sync": {
            "data": {
                "healthy": 1,
                "degraded": 0,
                "unhealthy": 0,
                "version": "7.4.4.92"
            },
            "status": "Healthy"
        },
        "arangodb-internal": {
            "data": {
                "version": "3.9.3",
                "license": "community",
                "server": "arango"
            },
            "status": "Healthy"
        },
        "arangoDB": {
            "data": {
                "updatedAt": "2024-05-08T12:57:42.9260267Z",
                "failureStatus": "Unhealthy",
                "status": "Healthy"
            },
            "status": "Healthy"
        },
        "masstransit-bus": {
            "data": {
                "endpoints": {
                    "rabbitmq://rabbitmq.dev.env.av360.org/maverick.user-profile.api-submit-command-response": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://rabbitmq.dev.env.av360.org/maverick.user-profile.api-health-check-message.consumer?temporary=true": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://rabbitmq.dev.env.av360.org/f7e26e9d21dc_UserProfileServiceCustom_bus_yryyyyfcncbrfp53bdqgsh19rf?temporary=true": {
                        "status": "Healthy",
                        "description": "ready (not started)"
                    }
                }
            },
            "status": "Healthy"
        },
        "redis-internal": {
            "data": {},
            "status": "Healthy"
        },
        "redis": {
            "data": {
                "updatedAt": "2024-05-08T12:57:42.9260362Z",
                "failureStatus": "Degraded",
                "status": "Healthy"
            },
            "status": "Healthy"
        }
    },
    "status": "Healthy",
    "version": "7.4.4.92"
}
```

They display the health status of the utilized third-party components and also show the current version of the service. Additionally, they indicate the health status of the UPS components: the **SagaWorker** and the **Sync**. The Sync also offers a `health/state` endpoint. You can also, of course, utilize the other health endpoints.