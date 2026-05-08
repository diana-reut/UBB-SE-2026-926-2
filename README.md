### How to load database

### 1. Humble Begginings
In fisierul appsettings.local.json daca ati mai lucrat pe o baza de date existenta adaugati la final "EF" la nume cat sa genereze alta. Daca nu lasati pe ce era original. Daca nu aveti fisierul un exemplu de cum trebuie sa arate se gaseste in appsettings.example.json
![Imi bag \*\*\*\*](image.png)

### 2. Making some progress
Copiati connection stringul din appsettings.local.json si aici (Common.Data / HospitalDBContextFactory):
![Copy inside of appsettings.local.json](image-1.png)

### 3. Getting commando
Open this O_O
![Open Nuget Console](image-2.png)

### 4. Setting up
Selectati proiectul corect in care sa rulam comenzile.
![Click on Common.Data](image-3.png)

### 5. Running Db Setup
Rulati "Update-Database" in consola. Daca tot e ok o sa aveti un output similar sau un output extrem de lung dar fara nimica error sau rosu.
![Update-Database](image-4.png)

### 6. Seed Data
Open SSMS si rulati codul din fisierul ssms_seed_data_common_initialcreate_rerunnable_valid_cnp.sql .



### TO RUN:

Open the HospitalManagement project (NO FOLDER VIEW, use the sln for the whole project) and run HospitalManagement in Unpackaged mode.
