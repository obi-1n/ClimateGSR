# ClimateGSR

ClimateGSR is a web-based climate awareness and quiz application built with ASP.NET MVC. It combines an interactive world map with country-level emissions and climate data, allowing users to explore statistics and test their knowledge through a short game.

## Overview

The project is designed to help users learn about climate impact across countries by:

- viewing country climate and demographic data on an interactive map
- comparing emissions and population statistics for different countries
- playing a quiz that asks users to identify countries from emissions prompts
- tracking score progress and leaderboard results for registered players

## Features

- Interactive Leaflet map of countries
- Country selection panel with climate and demographic metrics
- Quiz-style game flow with hints, scoring, and final results
- Leaderboard for high scores and lifetime points
- ASP.NET Identity-based user accounts and saved progress
- Entity Framework-backed data model for countries, continents, questions, and game history

## Tech Stack

- ASP.NET MVC 5
- .NET Framework 4.7.2
- C#
- Entity Framework 6
- Microsoft SQL Server LocalDB
- Bootstrap and Leaflet.js

## Project Structure

- `Controllers/` – MVC controllers for the home page, game flow, accounts, and leaderboard
- `Models/` – domain models, database context, scoring logic, and question generation
- `Views/` – Razor front-end pages for the web UI
- `Content/` and `Scripts/` – styling and client-side scripts
- `App_Start/` – application startup and routing configuration

## Prerequisites

Before running the application, make sure you have:

- Visual Studio 2019 or later with ASP.NET web development tools
- .NET Framework 4.7.2 support
- SQL Server LocalDB installed and available on the machine
- NuGet package restore enabled

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/obi-1n/ClimateGSR.git
   cd ClimateGSR
   ```

2. Open `ClimateGSR.sln` in Visual Studio.

3. Restore NuGet packages.

4. Build the solution.

5. Run the project with IIS Express or F5.

6. The application will connect to the LocalDB instance configured in `Web.config` and initialize the required database objects as needed.

## Running the App

Once started, the app presents a climate explorer and game experience accessible through the browser. Users can:

- explore the map and select countries
- play the climate guessing game
- view the leaderboard and progress after signing in

## Notes

This repository contains a classroom/group project structure and is intended as a demo of a climate education application using ASP.NET MVC and web mapping tools.
