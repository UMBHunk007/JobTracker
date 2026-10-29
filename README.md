<h1 align="center">🧭 JobTracker</h1>

<p align="center">
  <i>A job-offer tracking API that imports listings, organizes your applications, and scores them against your CV.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 10"/>
  <img src="https://img.shields.io/badge/C%23-ASP.NET%20Core-239120?style=flat-square&logo=csharp&logoColor=white" alt="C# ASP.NET Core"/>
  <img src="https://img.shields.io/badge/SQL%20Server-EF%20Core%2010-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white" alt="SQL Server"/>
  <img src="https://img.shields.io/badge/Status-In%20Progress-orange?style=flat-square" alt="In progress"/>
</p>

---

## 📖 About

**JobTracker** is a portfolio backend project built with **ASP.NET Core Web API, C#, and SQL Server**. It imports job offers from an external source, stores them, tracks the status of each application, and extracts skills from a PDF résumé to give a rough compatibility score for every offer.

It was built to practice backend design, data persistence, web scraping, and basic document parsing.

> **Note:** JobTracker helps you review and organize offers. It does **not** submit applications automatically.

## ✨ Features

- **Computrabajo import:** fetches offers from search results and parses the details page of each one.
- **Persistence with EF Core + SQL Server:** offers are stored and managed through the API.
- **Duplicate control:** unique indexes on *source + external ID* and on *source + URL*.
- **Application tracking:** each offer moves through the statuses `Pending`, `Ready`, `Skipped`, and `Applied`.
- **PDF résumé analysis:** extracts text from a CV and detects skills to build a candidate profile.
- **Heuristic match score:** a reference score based on detected skills. It is *not* an AI model or a semantic evaluation.
- **Static review page:** a simple UI at `wwwroot/index.html`.
- **Swagger / OpenAPI:** explore and test every endpoint in development.

## 🧰 Tech Stack

| Area | Technology |
|------|------------|
| Language / Runtime | C# · .NET 10 |
| Framework | ASP.NET Core Web API |
| Database | SQL Server (Express or compatible) · Entity Framework Core 10 |
| HTML parsing | HtmlAgilityPack |
| PDF parsing | PdfPig |
| API docs | Swagger (Swashbuckle) · OpenAPI |

## ⚙️ How It Works

1. **Import:** a search query is sent to Computrabajo and the results are parsed into offers.
2. **Store:** offers are saved to SQL Server, skipping duplicates.
3. **Profile:** a PDF résumé is uploaded, its text is extracted, and skills are detected.
4. **Score:** each offer gets a heuristic compatibility score based on matching skills.
5. **Track:** you review offers and move them through `Pending → Ready → Applied` (or `Skipped`).

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server Express, configured to accept your chosen authentication
- `dotnet-ef` (to run migrations from the CLI)

### Installation

1. **Clone the repository**

   ```powershell
   git clone https://github.com/UMBHunk007/JobTracker.git
   cd JobTracker/JobTracker.Api
   ```

2. **Create your local configuration**

   ```powershell
   Copy-Item .\appsettings.example.json .\appsettings.json
   ```

3. **Set your connection string** in `appsettings.json`. The template targets a local SQL Server Express instance (`.\SQLEXPRESS`) with integrated authentication.

   > `appsettings.json` is excluded from Git. Never commit passwords, tokens, or private connection strings. Only `appsettings.example.json` is versioned.

4. **Restore dependencies**

   ```powershell
   dotnet restore
   ```

5. **Install the EF Core CLI** (if you don't have it)

   ```powershell
   dotnet tool install --global dotnet-ef --version 10.0.13
   ```

6. **Apply the migrations**

   ```powershell
   dotnet ef database update
   ```

7. **Run the API**

   ```powershell
   dotnet run --urls http://localhost:5131
   ```

8. **Open Swagger** (development only): <http://localhost:5131/swagger>

## 🔌 Usage

Import offers from Computrabajo with a search query:

```http
POST http://localhost:5131/api/computrabajo/import?title=desarrollador&page=1
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/computrabajo/import` | Import offers by title and page |
| [...]  | [...]    | [Add the rest of your endpoints] |

> Swagger lists every available endpoint with request and response examples.

## 📁 Project Structure

```text
JobTracker/
├── .gitignore
├── README.md
└── JobTracker.Api/
    ├── Controllers/              # API endpoints
    ├── Data/                     # DbContext and EF Core configuration
    ├── Migrations/               # Database migrations
    ├── Models/                   # Entities and DTOs
    ├── Services/                 # Import, CV parsing, scoring
    ├── wwwroot/                  # Static review page
    ├── appsettings.example.json  # Safe configuration template
    ├── JobTracker.Api.csproj
    └── Program.cs
```

## 📸 Screenshots

<!-- Add a screenshot of Swagger and of the review page (wwwroot/index.html) -->
<!-- ![Swagger](docs/swagger.png) -->

## 🗺️ Roadmap

- [ ] [Additional job sources]
- [ ] [Better skill matching]
- [ ] [Automated tests]
- [ ] [Docker setup]

## ⚠️ Disclaimer

- The match score is a rough heuristic and must not be read as a prediction of being hired.
- Scraping depends on the HTML structure and internal endpoints of external sites, which can change or rate-limit requests. The importer may need maintenance if Computrabajo changes.
- Respect the terms of use and access limits of any external source.

## 📄 License

[Choose a license, e.g. MIT]. See [LICENSE](LICENSE).