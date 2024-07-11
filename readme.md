# NicoScraper

## About
This is a scraping utility I wrote in .Net to track inventory and stock prices across a few different websites.

At the time I wrote this, I was only tracking a single website (QuitMed), but the project was set up with extension in mind.

The project includes a WebApp written in Blazor which displays the data in a formatted fashion, and runs an actor system in the background to re-scrape every 24 hours.

**This project is working as of 5th of June, 2024**. Due to the nature of scraping, website updates since then may have broken this.

It includes a compose file to get it running quickly locally.

I also included Bicep files as I was intending on deploying this to Azure, but since abandoned the project.

## Technologies used
- .Net
- Blazor
- Entity Framework
- Akka.NET
- Docker Compose
- Bicep