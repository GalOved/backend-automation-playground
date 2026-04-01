# Async Scan QA Platform

A distributed backend QA automation project that simulates an asynchronous document scanning system, inspired by real-world architectures like Copyleaks.

---

## 🚀 Overview

This project demonstrates how to build and test a **distributed, event-driven backend system** with asynchronous processing.

The system accepts scan requests, processes them via a queue-based worker, and exposes APIs and a dashboard to monitor scan status.

It is designed as a hands-on learning project to practice backend QA automation, system design, and infrastructure skills.

---

## 🧱 Architecture

```text
Client / Tests
      ↓
Scan API (C#)
      ↓
   Queue (RabbitMQ / Redis)
      ↓
Worker (Python)
      ↓
Storage (SQLite / JSON)
      ↓
Mini Dashboard (UI)
```

---

## 🛠️ Tech Stack

* **Backend API:** C# (.NET, ASP.NET Core)
* **Worker Service:** Python
* **Queue / PubSub:** RabbitMQ or Redis
* **Testing:**

  * Python (pytest, requests/httpx)
  * Playwright (E2E & UI testing)
* **Containerization:** Docker + Docker Compose
* **Orchestration:** Kubernetes
* **Version Control:** Git
* **External Integration (Optional):** Copyleaks API (Sandbox)

---

## 📦 Features

* Submit scan requests via REST API
* Asynchronous processing using a queue
* Worker-based scan execution
* Status lifecycle:

  * `PENDING`
  * `PROCESSING`
  * `COMPLETED`
  * `FAILED`
* Scan result tracking
* Mini dashboard for monitoring scans
* End-to-end automated tests

---

## 🔁 Scan Flow

1. Client sends a request to `POST /scans`
2. API creates a new scan (`PENDING`)
3. Message is published to the queue
4. Worker consumes the message
5. Status updated to `PROCESSING`
6. Worker completes processing
7. Final status set to `COMPLETED` or `FAILED`
8. Results are stored and exposed via API/UI

---

## 📂 Project Structure

```text
copyleaks-qa-prep/
│
├── services/
│   ├── scan-api/          # C# API service
│   ├── worker/            # Python worker
│   └── mini-dashboard/    # UI (Playwright-tested)
│
├── tests/
│   ├── python-api-tests/  # API & integration tests
│   ├── csharp-tests/      # C# unit/integration tests
│   └── playwright-e2e/    # UI & E2E tests
│
├── infra/
│   ├── docker/            # Dockerfiles
│   └── k8s/               # Kubernetes manifests
│
├── docs/                  # Design & notes
│
└── README.md
```

---

## ⚙️ Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/async-scan-qa-platform.git
cd async-scan-qa-platform
```

---

### 2. Run with Docker Compose

```bash
docker compose up --build
```

Services will be available locally:

* API: http://localhost:5000
* Dashboard: http://localhost:3000 (if applicable)

---

### 3. Run tests

#### Python tests

```bash
cd tests/python-api-tests
pytest
```

#### Playwright tests

```bash
cd tests/playwright-e2e
npx playwright test
```

---

## ☸️ Kubernetes Deployment

Apply manifests:

```bash
kubectl apply -f infra/k8s/
```

Useful commands:

```bash
kubectl get pods
kubectl logs <pod-name>
kubectl describe pod <pod-name>
```

---

## 🧪 Testing Strategy

### API Tests

* Create scan
* Get scan by ID
* Validate input errors
* Status transitions

### Integration Tests

* End-to-end async flow
* Queue processing validation
* Worker behavior

### Playwright (E2E)

* Dashboard rendering
* Scan lifecycle visualization
* Error handling scenarios

---

## 🔐 Environment Variables

Use a `.env` file for configuration:

```env
QUEUE_HOST=localhost
QUEUE_PORT=5672
API_PORT=5000
```

> ⚠️ Do not commit secrets or API keys

---

## 🔌 Copyleaks Integration (Optional)

The system can integrate with the Copyleaks API (Sandbox mode) to simulate real document scanning.

Steps:

* Authenticate using API key
* Submit scan request
* Handle async processing
* Parse results

---

## 📘 Learning Goals

This project focuses on:

* Asynchronous system design
* Queue-based architectures
* Backend QA automation
* API testing strategies
* Distributed system debugging
* Docker & Kubernetes workflows
* Writing maintainable test suites

---

## ✅ Definition of Done

* API service implemented (C#)
* Worker service implemented (Python)
* Queue integration working
* Storage layer functional
* Dashboard available
* Python tests passing
* Playwright tests passing
* Docker Compose setup working
* Kubernetes deployment working

---

## 👨‍💻 Author

Built as a backend QA automation preparation project.

---

## 📌 Notes

This is a learning project designed to simulate real-world backend systems and testing strategies.
