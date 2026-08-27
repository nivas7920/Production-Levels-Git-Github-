# Production-Levels-Git-Github

A practical repository for learning and implementing **production-level Git, GitHub, CI/CD, Docker, and .NET development practices**.

## 📌 Project Overview

This repository contains a .NET-based application along with examples and configurations related to modern software development and deployment workflows.

The main goal of this project is to understand how development teams manage source code, branches, pull requests, automated builds, Docker containers, and CI/CD pipelines in a production environment.

## 🛠️ Technologies Used

* .NET
* C#
* ASP.NET Core
* Git
* GitHub
* GitHub Actions
* Docker
* CI/CD
* SQL Server
* Visual Studio

## 📂 Project Structure

```text
Production-Levels-Git-Github/
│
├── OnlineShoppingApplication/
│   └── Main application
│
├── OnlineShoppingApplication.Tests/
│   └── Unit tests
│
├── .gitignore
│
├── OnlineShoppingApplication.slnx
│
└── README.md
```

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
```

### 2. Open the Project

Open the solution in **Visual Studio**.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Build the Application

```bash
dotnet build
```

### 5. Run Tests

```bash
dotnet test
```

### 6. Run the Application

```bash
dotnet run
```

## 🌿 Git Branching Strategy

The repository can follow a standard Git workflow:

```text
main
 │
 ├── feature/*
 │
 ├── bugfix/*
 │
 └── hotfix/*
```

### Main Branch

The `main` branch contains stable code that is ready for production or release.

### Feature Branch

Used when developing a new feature.

```bash
git checkout -b feature/add-product
```

### Bugfix Branch

Used for fixing defects.

```bash
git checkout -b bugfix/fix-payment-issue
```

## 🔄 Development Workflow

The typical development workflow is:

```text
Developer
    ↓
Create Feature/Bugfix Branch
    ↓
Develop & Test
    ↓
Commit Changes
    ↓
Push to GitHub
    ↓
Create Pull Request
    ↓
Code Review
    ↓
CI Pipeline
    ↓
Merge
    ↓
Build / Test / Docker
    ↓
Deployment
```

## 🔁 CI/CD Pipeline

GitHub Actions can be used to automate:

* Code checkout
* .NET SDK setup
* Dependency restoration
* Application build
* Unit testing
* Docker image creation
* Docker image push
* Deployment

Example pipeline stages:

```text
Checkout
   ↓
Setup .NET
   ↓
Restore
   ↓
Build
   ↓
Test
   ↓
Docker Build
   ↓
Docker Push
   ↓
Deployment
```

## 🐳 Docker

Docker is used to package the application and its dependencies into a container.

Typical commands:

```bash
docker build -t online-shopping-app .
```

Run the container:

```bash
docker run -d -p 8080:8080 online-shopping-app
```

## 🧪 Testing

The project contains a separate test project:

```text
OnlineShoppingApplication.Tests
```

Tests can be executed using:

```bash
dotnet test
```

## 🔐 Production-Level Git Practices

Important practices followed in this repository include:

* Use meaningful branch names
* Write clear commit messages
* Create Pull Requests for changes
* Perform code reviews
* Avoid directly pushing unfinished code to `main`
* Keep commits small and meaningful
* Run tests before creating a Pull Request
* Use CI/CD pipelines for automated validation
* Never commit passwords, API keys, or connection strings
* Use environment variables and secrets for sensitive configuration

## 📝 Commit Message Examples

Good commit messages:

```text
feat: add product search
fix: resolve payment validation issue
test: add unit tests for order service
docs: update README
ci: add Docker build pipeline
refactor: improve product service
```

## 📚 Learning Objectives

This repository is intended to provide practical experience with:

1. Git fundamentals
2. Git branching and merging
3. GitHub repositories
4. Pull Requests
5. Code Reviews
6. GitHub Actions
7. CI/CD pipelines
8. Docker
9. .NET application deployment
10. Production-level development workflow

## 👨‍💻 Author

**Nivas Bidave**

.NET Developer

---

⭐ This repository is continuously updated while learning and implementing production-level development practices.
