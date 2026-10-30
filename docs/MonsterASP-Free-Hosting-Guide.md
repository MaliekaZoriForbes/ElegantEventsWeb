# Host Elegant Events on MonsterASP.NET (Free) — Full Guide

**Project:** ElegantEventsWeb (ASP.NET Core MVC + SQL Server)  
**Goal:** A public URL (for example `https://yourname.runasp.net`) you can give your lecturer.  
**Host:** [MonsterASP.NET](https://www.monsterasp.net/) free plan  
**Guide based on:** MonsterASP help docs (updated October 2026)

---

## What you get on the Free plan

| Item | Free allowance |
| --- | --- |
| Website | 1 site, 5 GB storage, 256 MB RAM, EU datacenter |
| Database | 1× SQL Server **2025**, 1 GB storage, EU |
| Address | Free subdomain: `*.runasp.net` or `*.tryasp.net` |
| Cost | $0 — no credit card, no ads injected into your pages |
| Deploy | Visual Studio Web Deploy, FTP/SFTP, ZIP, or GitHub (manual Pull & Publish) |

**Important for this project**

1. Free plan = **one** database. Point **both** `DefaultConnection` and `IdentityConnection` at that **same** database.
2. The app uses `CookieSecurePolicy.Always`, so **HTTPS is required** for login to work reliably. On Free, request HTTPS via a support ticket (see Part 7).
3. Hosting is for the **live demo**. You still submit the assignment package (report, SQL docs, CD/export, PowerPoint) separately.

---

## Part 0 — Prerequisites

Before you start, have:

1. A working email address  
2. **VPN/proxy turned off** during signup (MonsterASP blocks VPN registrations)  
3. Visual Studio with the **ASP.NET and web development** workload  
4. This solution open: `ElegantEventsWeb`  
5. Your business SQL scripts ready (tables, views, stored procedures, functions, triggers) — or the ability to recreate them  
6. Optional but useful: [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/ssms/install/install) or Azure Data Studio  

---

## Part 1 — Create a free MonsterASP account

1. Open [https://www.monsterasp.net/](https://www.monsterasp.net/).
2. Find **Create your account**.
3. Enter your email address, wait for human verification to finish, then click **Create account**.
4. Check your inbox (and spam folder) for:
   - Control Panel login details
   - Temporary password
   - Activation code
5. On the **Activate your account** page:
   - Enter full name (or company name)
   - Choose country
   - Enter mobile phone in international format (for example `+1...`)
   - Paste the activation code
   - Click **Activate account**

You should land in the Control Panel automatically.

**Sign in later:** use the Control Panel link from the registration email (or the Control Panel link on the MonsterASP site).

**Official docs:** [Create your account](https://help.monsterasp.net/getting-started/first-steps/create-account)

---

## Part 2 — Create the free website

1. In the Control Panel, go to **Websites** → **Add website** (or **New website** on the Dashboard).
2. Click **Select FREE website**.
3. Under **Free subdomain**:
   - Type a website name (minimum 3 characters: letters `a–z`, numbers `0–9`, and dash `-`)
   - Choose `runasp.net` or `tryasp.net`
   - Example result: `elegantevents.runasp.net`
   - **Write this URL down** — this is what you give the lecturer
4. Choose the **EU** datacenter (Free is EU-only) → **Continue**.
5. For the example application, choose **My own application** (empty site).  
   Do **not** install a demo app unless you want to delete its files later.
6. Check the Summary → click **Create Website**.
7. Wait until the site appears in your Websites list.

**Optional runtime check:** open the website → **Scripting**. ASP.NET / .NET should be set to modern .NET (8+). This project targets **.NET 10**. MonsterASP Free supports .NET 8, 9, 10, and 11.

**Official docs:** [Create your first website](https://help.monsterasp.net/getting-started/first-steps/create-first-website)

---

## Part 3 — Create the free SQL Server database

1. Control Panel → **Databases** → **Add database**.
2. Click **Select FREE database**.
3. Choose database type: **SQL Server 2025**.
4. Collation: leave the default `SQL_Latin1_General_CP1_CI_AS` unless your local database used a different collation.
5. Continue → **Select EU datacenter** (same region as the website).
6. Click **Create Database**.
7. Wait until it appears in the Databases list.

**Rule:** Create the website and database on the **same plan** (both Free). A Free website cannot use the local connection string of a Premium database (and the reverse).

**Official docs:** [Create an MSSQL database](https://help.monsterasp.net/databases/mssql/create-database)

---

## Part 4 — Copy the connection strings (local vs remote)

Every MonsterASP database has **two** connection strings. Using the wrong one is the most common failure.

1. Go to **Databases** → open your database → **Users**.
2. Click **Connection string** next to the main user.
3. Save **both** strings somewhere private (password manager / secure note):

| Where the app / tool runs | Which string to use |
| --- | --- |
| Website hosted on MonsterASP | **Local** connection string |
| Your PC (SSMS, `dotnet ef`, scripts) | **Remote** connection string (after enabling remote access) |

A remote connection string looks like:

```text
Server=dbXXXX.public.databaseasp.net; Database=dbXXXX; User Id=dbXXXX; Password=your-password; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;
```

**Keep `TrustServerCertificate=True`.** Without it you often get:

> The certificate chain was issued by an authority that is not trusted

**Official docs:** [Find your connection string](https://help.monsterasp.net/databases/mssql/connection-string)

---

## Part 5 — Enable remote access (load schema from your PC)

By default the database only accepts connections from MonsterASP websites.

1. Open your database → **Users**.
2. In the **Remote access** panel, switch remote access to **Enabled**.
3. Note the public server name shown in the **Server** row (it contains the word `public`, for example `dbXXXX.public.databaseasp.net`).  
   Always use this hostname — **never** a raw IP (IPs can change).
4. Click **Test connection** in that panel.
5. Optional network check: open `http://dbXXXX.public.databaseasp.net` in a browser.  
   A green **Server is online** message means your network can reach the SQL host.  
   If it fails, try a mobile hotspot (some school/ISP networks block outbound SQL ports).

**Note:** Only the **main** database user can sign in from SSMS. Extra users you create are for connection strings only.

**Official docs:** [Allow remote access to a database](https://help.monsterasp.net/databases/mssql/remote-access)

---

## Part 6 — Load database schema + Identity tables

Free plan = **one** database. Put **business data and ASP.NET Identity** in that same database.

### 6A — Business SQL (tables, views, procedures, functions, triggers)

Choose one method:

#### Option A — Control Panel (good if you have a `.sql` file)

1. Databases → open your database  
2. Use **Import** and/or **Run T-SQL**  
3. Run your CREATE scripts  

#### Option B — SSMS / Azure Data Studio

1. Connect using the **remote** connection string / public server  
2. Log in with the main database user and password  
3. Run your scripts against that database  

Your hosted database needs at least the objects the web app uses, including:

**Tables (examples):** `client`, `eventt`, `venue`, `vendor`, `contract`, `payment`, `staff`, `held`, `works`, …

**Views:**

- `dbo.View_Client_Events`
- `dbo.View_Event_Staff`
- `dbo.View_Event_Vendors`
- `dbo.View_Event_Venues`
- `dbo.View_Wedding_Details`

**Stored procedures:**

- `dbo.tr_book_event_venue`
- `dbo.tr_record_payment`
- `dbo.SP_Staff_Event_Assign`
- `dbo.SP_Vendor_Contracts`

**Function:**

- `dbo.fn_contract_balance`

Also include your real **triggers** and **transactions** (commit/rollback) for assignment marks 1–4. Objects named `tr_*` that are called as stored procedures are **not** the same as SQL `CREATE TRIGGER` objects.

### 6B — ASP.NET Identity tables (EF Core migration)

From PowerShell, in the web project folder:

```powershell
cd c:\Work\ElegantEventsWeb\ElegantEventsWeb

dotnet ef database update --connection "PASTE_REMOTE_CONNECTION_STRING_HERE"
```

That applies `Migrations/20261029202845_InitialIdentity.cs` to the hosted database.

If the EF tool is missing:

```powershell
dotnet tool install --global dotnet-ef
```

---

## Part 7 — Configure the app for hosting

### 7A — Connection strings (same database twice)

Create or update `ElegantEventsWeb/appsettings.json` for the hosted environment.

Use the **LOCAL** connection string for both names (website → database on MonsterASP’s internal network):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PASTE_LOCAL_CONNECTION_STRING_HERE",
    "IdentityConnection": "PASTE_LOCAL_CONNECTION_STRING_HERE"
  },
  "AdminSetup": {
    "Email": "admin@elegantevents.local",
    "Password": "YourStrong!Pass1"
  },
  "CoordinatorSetup": {
    "Email": "coord@elegantevents.local",
    "Password": "YourStrong!Pass1"
  }
}
```

Password rules in this project (Identity):

- Minimum length 10  
- Require digit, uppercase, lowercase, and non-alphanumeric  

Keep local development strings in `appsettings.Development.json` if you still develop against LocalDB / a local SQL instance.

> `appsettings.json` is gitignored in this repo. That is intentional so secrets are not committed. Still make sure the published site gets the correct hosted settings (via the file you publish, or Visual Studio publish connection-string settings).

### 7B — Request HTTPS (do this before relying on login)

On Free, Let’s Encrypt HTTPS is **not** automatic.

1. Control Panel → **Tickets**
2. Open a ticket asking to enable HTTPS for your free website, for example:

   > Please enable HTTPS / Let’s Encrypt for my free website `yourname.runasp.net`.

3. Wait for support to activate it.

Without HTTPS, secure cookies (`CookieSecurePolicy.Always`) may not be set over plain HTTP, and login will appear broken.

### 7C — Create the first Administrator / Coordinator on the server

In `Program.cs`, Admin and Coordinator seeding runs **only when** `IsDevelopment()` is true. On a normal Production host those users will **not** auto-create (roles still create on every startup).

Pick one approach for the assignment:

1. **Temporary first boot:** if the Control Panel allows environment variables, set `ASPNETCORE_ENVIRONMENT=Development` once, open the site so users seed, then set it back to `Production`.
2. Create the users another way after deploy (SQL / Identity tooling).
3. Temporarily also run the seed blocks in Production, republish, then (optional) revert — acceptable for coursework, not ideal for real production.

---

## Part 8 — Publish from Visual Studio (recommended)

**Official docs:**

- [Publish your first application](https://help.monsterasp.net/getting-started/first-steps/publish-first-application)
- [ASP.NET Core with an MSSQL database](https://help.monsterasp.net/websites/frameworks/aspnet-core-with-mssql)

### Steps

1. Control Panel → **Websites** → your website → **Deploy**.
2. On the **Visual Studio access** card, switch **WebDeploy** to **Enabled**.
3. Click **Download publish profile** and save the `.publishSettings` file.
4. Open the solution in Visual Studio.
5. In Solution Explorer, right-click the **ElegantEventsWeb** project → **Publish**.
6. Choose **Import profile** and select the downloaded `.publishSettings` file.
7. Click **Show all settings**.
8. Expand **Database** and **Entity Framework Migrations**.  
   Paste the **LOCAL** connection string where required (use the same hosted DB string for both if Visual Studio only shows one EF context).
9. Save → click **Publish**.
10. Wait for success. The website is briefly stopped and restarted automatically.

Then open:

```text
https://yourname.runasp.net
```

(Use `http://` only until HTTPS is enabled.)

### Alternate deploy methods (if Web Deploy fails)

| Method | When to use |
| --- | --- |
| FTP / SFTP | Publish to a local folder (`dotnet publish -c Release`), upload contents into `\wwwroot` |
| ZIP upload | Upload a zip in the File manager and extract into `\wwwroot` |
| GitHub | Connect repo; Free allows manual Pull & Publish (auto-deploy on push is Premium) |

**Important:** Files must go into `\wwwroot`. Files outside that folder are not served.

---

## Part 9 — Smoke-test before sending the link

Work through this checklist on the live site:

1. Home page loads (not HTTP 500)
2. Login works with the Administrator account
3. Dashboard counts load
4. Clients list / create works
5. Reports page loads (SQL views)
6. Create Event works (stored procedure `tr_book_event_venue`)
7. Payments works as Administrator (`tr_record_payment` + `fn_contract_balance`)
8. Coordinator cannot open Payments (role restriction)

### What to send the lecturer

- Live URL: `https://yourname.runasp.net`
- Demo accounts (email + password) in a **private** note / report appendix  
  Do **not** put live passwords in a public GitHub README

---

## Part 10 — Hosting vs assignment hand-in

A hosted URL is for the **live demo**. The assignment still expects:

1. Final report (cover page, numbered contents, components 1–6, SQL documentation for items 1–4)
2. Application package using Export/Import (CD / USB / zip: publish output + `.sql` scripts + optional `.bak`)
3. Appendix: Logical Database Design from the DB Design assignment
4. Week-11 PowerPoint presentation with the implemented database

---

## Part 11 — Troubleshooting

| Symptom | Likely cause / fix |
| --- | --- |
| HTTP 500 after publish | Wrong or missing connection string; broken JSON in `appsettings.json`. Check Control Panel application / access logs. |
| Site cannot reach database | Use the **local** string on the server, not the remote string. Confirm website and DB are both Free. |
| SSMS cannot connect | Enable remote access; use `*.public.databaseasp.net`; try another network/hotspot. |
| Login does not stick / redirect loops | HTTPS not enabled while cookies require secure. Open a HTTPS support ticket. |
| No admin user after deploy | Seed is Development-only. See Part 7C. |
| Errors about missing SP / view / function | Business schema not imported into the hosted DB. Re-run Part 6A. |
| Files locked during publish | Retry publish; or Stop the site on Overview, publish, then Start. |
| Certificate trust error from PC | Add `TrustServerCertificate=True` to the remote connection string. |
| IP blocked after bad passwords | Too many failed logins can block your IP for 24 hours. Wait or use another network; fix credentials first. |

---

## Part 12 — Free-plan limits (set expectations)

- EU only; shared infrastructure; limited traffic; **no SLA**
- No automatic daily backups on Free
- No custom domain on Free
- No outbound email on Free
- HTTPS: request via support ticket
- 256 MB RAM — fine for a class demo; do not stress-test with heavy load

MonsterASP themselves describe Free as best for testing / learning, not production guarantees. That is enough for an assignment demo link.

---

## Quick checklist

- [ ] Account created (VPN off)
- [ ] Free website created (`something.runasp.net` or `.tryasp.net`)
- [ ] Free SQL Server 2025 database created (EU)
- [ ] Remote access enabled
- [ ] Business schema imported
- [ ] Identity migration applied (`dotnet ef database update`)
- [ ] `appsettings.json` uses **local** string for `DefaultConnection` and `IdentityConnection`
- [ ] HTTPS ticket requested / approved
- [ ] Admin and Coordinator users exist on hosted DB
- [ ] Visual Studio Web Deploy publish succeeded
- [ ] Smoke tests passed
- [ ] Lecturer URL + demo logins prepared
- [ ] Assignment report / SQL pack / export package still prepared separately

---

## Official sources (October 2026)

- [Free ASP.NET hosting overview](https://www.monsterasp.net/ASP.NET-Freehosting/)
- [Create your account](https://help.monsterasp.net/getting-started/first-steps/create-account)
- [Create your first website](https://help.monsterasp.net/getting-started/first-steps/create-first-website)
- [Create an MSSQL database](https://help.monsterasp.net/databases/mssql/create-database)
- [Find your connection string](https://help.monsterasp.net/databases/mssql/connection-string)
- [Allow remote access](https://help.monsterasp.net/databases/mssql/remote-access)
- [Publish your first application](https://help.monsterasp.net/getting-started/first-steps/publish-first-application)
- [ASP.NET Core with MSSQL](https://help.monsterasp.net/websites/frameworks/aspnet-core-with-mssql)
- [Choose the .NET version (Scripting)](https://help.monsterasp.net/websites/configuration/scripting)
- [Control Panel tour](https://help.monsterasp.net/getting-started/control-panel-basics/control-panel-tour)

---

*Document prepared for ElegantEventsWeb assignment hosting. Keep connection strings and demo passwords out of public repositories.*
