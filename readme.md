# NicoScraper

## Overview
NicoScraper is a comprehensive .NET-based utility designed for scraping inventory and stock prices from various websites, initially focusing on QuitMed. It leverages a Blazor WebApp for data presentation and utilizes an actor system for periodic data scraping, ensuring up-to-date information is always available.

*Note: This project was last verified to work on June 5, 2024. Due to the dynamic nature of web scraping, changes to target websites may require updates to the scraping logic.*

## Features
- **Data Scraping**: Automated scraping of inventory and stock prices from designated websites.
- **Blazor WebApp**: A user-friendly interface for data visualization.
- **Actor System**: Utilizes Akka.NET to manage scraping processes efficiently.
- **Docker Support**: Includes Docker Compose for easy local setup.
- **Azure Ready**: Prepared Bicep files for potential Azure deployment.

## Technologies
- .NET 8.0
- Blazor
- Entity Framework
- Akka.NET
- Docker Compose
- Bicep

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose
- An IDE like Visual Studio or VS Code

### Running Locally
1. Clone the repository to your local machine.
2. Navigate to the project directory.
3. Use Docker Compose to build and run the application:
   ```sh
   docker-compose -f infra/compose.yaml up --build
   ```
4. Access the Blazor WebApp through your browser at the specified port.

### Project Structure
- src/Quitmed-Scraper.Console/: Console application for background scraping tasks.
- src/Quitmed-Scraper.Database/: Entity Framework Core database project.
- src/Quitmed-Scraper.Library/: Core library containing the scraping logic.
- src/Quitmed-Scraper.WebApp/: Blazor WebApp for data presentation.
- infra/: Infrastructure as Code (IaC) files for Docker and Azure deployment.