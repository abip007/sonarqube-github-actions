# SonarQube + GitHub Actions: Code Quality Analysis (.NET)

A sample **C# / .NET 8** project with **xUnit** tests. A **GitHub Actions** workflow builds and tests it, then analyzes it with **SonarQube** on every push. The results (Bugs, Code Smells, Coverage, Maintainability) show up on the SonarQube dashboard.

## Project Structure

```
sonarqube-github-actions/
├── .github/workflows/sonarqube.yml   # CI pipeline: build, test, SonarQube scan
├── src/SonarDemo/                    # Sample class library
│   ├── Calculator.cs                 #   arithmetic + grading logic (fully tested)
│   └── StringUtils.cs                #   string helpers (planted issues, partly untested)
├── tests/SonarDemo.Tests/            # xUnit tests + coverlet coverage
├── SonarQube.Analysis.xml            # Sonar scanner config (.NET's version of sonar-project.properties)
├── sonarqube-server/
│   ├── docker-compose.yml            # Run SonarQube locally (Option B)
│   └── ec2-user-data.sh              # Run SonarQube on AWS EC2 (Option A)
└── SonarDemo.sln
```

> **Why `SonarQube.Analysis.xml` and not `sonar-project.properties`?**
> For C#, you must use the **SonarScanner for .NET** (`dotnet sonarscanner`), and it does **not** read `sonar-project.properties`. If it finds that file in the repo, it stops with an error. `SonarQube.Analysis.xml` is the .NET equivalent and holds the same settings (`sonar.host.url`, the token, report paths). `sonar.projectKey` is passed with `/k:` in the workflow.

## Pipeline Flow

```
git push ──► GitHub Actions
              ├─ checkout (full history)
              ├─ setup Java 17 + .NET 8
              ├─ dotnet sonarscanner begin   (key, host URL, token)
              ├─ dotnet build
              ├─ dotnet test  ──► coverage.opencover.xml + .trx
              └─ dotnet sonarscanner end  ──► uploads report ──► SonarQube dashboard
```

---

## Step 1: Prepare the Environment (SonarQube server)

GitHub-hosted runners run in GitHub's cloud and **cannot reach `localhost` on your PC**. Pick one option:

| | Option A: EC2 (recommended) | Option B: Local Docker + self-hosted runner |
|---|---|---|
| Where SonarQube runs | AWS EC2, public IP | Docker Desktop on your PC |
| Who runs the workflow | GitHub-hosted `ubuntu-latest` | A runner installed on your PC |
| Cost | ~$0.04/hr (t3.medium) | Free |

### Option A: SonarQube on AWS EC2

1. EC2 console → **Launch instance**
   - AMI: **Ubuntu 24.04**, type: **t3.medium** (SonarQube needs at least 4 GB RAM), storage: 20 GB
   - Security group inbound: **SSH 22** (My IP) and **Custom TCP 9000** (0.0.0.0/0)
   - **Advanced details → User data**: paste the contents of [sonarqube-server/ec2-user-data.sh](sonarqube-server/ec2-user-data.sh)
2. Wait about 3–5 minutes, then open `http://<EC2-PUBLIC-IP>:9000`.
3. Your `SONAR_HOST_URL` is `http://<EC2-PUBLIC-IP>:9000`.

### Option B: SonarQube on local Docker

```powershell
cd sonarqube-server
docker compose up -d
docker logs -f sonarqube      # wait for "SonarQube is operational"
```
Open http://localhost:9000. Your `SONAR_HOST_URL` is `http://localhost:9000`.
You also need the **self-hosted runner** described in Step 5.

### First login
- Log in with `admin` / `admin`. SonarQube asks you to set a new password.

## Step 2: Sample Project (already created here)

- `src/SonarDemo/Calculator.cs`: Add, Subtract, Multiply, Divide, Grade, Average
- `src/SonarDemo/StringUtils.cs`: Reverse, IsPalindrome, CountVowels, plus **planted issues** so the report has findings:
  - unused variable (code smell)
  - catching generic `Exception` (code smell)
  - hard-coded password (security hotspot)
  - methods with no tests (lower coverage)
- `tests/SonarDemo.Tests/`: 16 xUnit tests

Run locally:
```powershell
dotnet build
dotnet test --collect:"XPlat Code Coverage;Format=opencover"
```

## Step 3: Configure SonarQube (project + token)

1. Dashboard → **Create Project → Local project**
   - Project display name: `SonarQube GitHub Actions Demo`
   - **Project key: `SonarQube-GitHub-Actions-Demo`** (must match `SONAR_PROJECT_KEY` in the workflow)
   - Main branch: `main` → **Next** → *Use the global setting* → **Create project**
2. Analysis method → choose **With GitHub Actions**. SonarQube shows you the values you need.
3. Generate a token:
   - Top-right avatar → **My Account → Security → Generate Tokens**
   - Name: `github-actions`, Type: **Project Analysis Token**, Project: the one above, Expires: 30 days
   - Click **Generate** and **copy the token now**. It is shown only once.

## Step 4: Scanner Configuration

[SonarQube.Analysis.xml](SonarQube.Analysis.xml) contains:

| Setting | Value |
|---|---|
| `sonar.projectKey` | passed via `/k:` in the workflow (`SonarQube-GitHub-Actions-Demo`) |
| `sonar.host.url` | `http://localhost:9000`, overridden in CI by the `SONAR_HOST_URL` secret |
| `sonar.token` (formerly `sonar.login`) | **never committed**; comes from the `SONAR_TOKEN` secret |
| `sonar.cs.opencover.reportsPaths` | coverage file produced by coverlet |
| `sonar.cs.vstest.reportsPaths` | `.trx` test results |

## Step 5: GitHub Repository + Actions Workflow

### 5.1 Create the GitHub repo
1. github.com → **New repository** → name: `sonarqube-github-actions` → **Public** → *don't* add a README → **Create**.
2. Push this project:
```powershell
cd c:\DevOps\sonarqube-github-actions
git remote add origin https://github.com/<your-username>/sonarqube-github-actions.git
git push -u origin main
```
(The first run will fail until the secrets are added. That's expected.)

### 5.2 Add the secrets
Repo → **Settings → Secrets and variables → Actions → New repository secret**:

| Name | Value |
|---|---|
| `SONAR_TOKEN` | the token from Step 3 |
| `SONAR_HOST_URL` | `http://<EC2-PUBLIC-IP>:9000` (Option A) or `http://localhost:9000` (Option B) |

### 5.3 Option B only: self-hosted runner on your PC
1. Repo → **Settings → Actions → Runners → New self-hosted runner → Windows x64**
2. Run the commands GitHub shows in PowerShell (download, `config.cmd`, then `run.cmd`). Keep the window open.
3. Repo → **Settings → Secrets and variables → Actions → Variables tab → New variable**:
   `RUNNER` = `self-hosted`
   The workflow uses `runs-on: ${{ vars.RUNNER || 'ubuntu-latest' }}`, so this one variable switches it to your PC.

### 5.4 The workflow
[.github/workflows/sonarqube.yml](.github/workflows/sonarqube.yml) runs on every push, on PRs, and manually. Its steps:
1. **Checkout** with `fetch-depth: 0`
2. **Set up Java 17** (the scanner engine needs it) and **.NET 8**
3. **Install** `dotnet-sonarscanner` and **restore** NuGet packages
4. `sonarscanner begin` → `dotnet build` → `dotnet test` (with coverage) → `sonarscanner end`

## Step 6: Run the Workflow

```powershell
git commit --allow-empty -m "Trigger SonarQube analysis"
git push
```
Repo → **Actions** tab → open **SonarQube Analysis**. All steps should be green. The *end analysis* log should end with:
```
ANALYSIS SUCCESSFUL, you can find the results at: http://<host>:9000/dashboard?id=SonarQube-GitHub-Actions-Demo
```

## Step 7: Verify the Report

Open SonarQube → **Projects → SonarQube GitHub Actions Demo**. You should see:

| Metric | Where | Expected |
|---|---|---|
| **Bugs / Reliability** | Overview → Reliability | rating shown |
| **Code Smells / Maintainability** | Overview → Maintainability | several (unused variable, generic catch, …) |
| **Security Hotspots** | Overview → Security Review | hard-coded password |
| **Coverage** | Overview → Coverage | about 60–70% (some methods untested on purpose) |
| **Maintainability Rating** | Overview | A–E rating |

Click **Issues** to see each finding with its file and line.

> Overview shows **New Code** by default. Switch to the **Overall Code** tab to see the full numbers.

## Step 8: Submit

1. GitHub repository link: `https://github.com/<your-username>/sonarqube-github-actions`
2. Screenshot of the SonarQube project **Overview (Overall Code)** page
3. Optional: a screenshot of the green GitHub Actions run

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `Could not connect to the SonarQube server` | `SONAR_HOST_URL` wrong, or port 9000 not open in the security group. Localhost only works with Option B. |
| `401 / Not authorized` | Token is wrong or expired. Regenerate it and update the `SONAR_TOKEN` secret. |
| `sonar-project.properties files are not understood` | Delete `sonar-project.properties`. The .NET scanner uses `SonarQube.Analysis.xml`. |
| Coverage shows 0% | Make sure `dotnet test` used `--collect:"XPlat Code Coverage;Format=opencover"` and runs **between** begin and end. |
| SonarQube container exits (EC2) | `vm.max_map_count` too low. The user-data script sets it; check with `sysctl vm.max_map_count`. |
| Job stuck "Waiting for a runner" | Option B: the self-hosted runner (`run.cmd`) isn't running. |

## Cleanup
- Option A: **terminate the EC2 instance** to stop charges.
- Option B: `docker compose down -v` in `sonarqube-server/`.
