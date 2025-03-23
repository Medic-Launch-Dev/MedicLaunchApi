Features that should work so far:

- Get all specialities
- Author can add a question to a speciality
- Author can update a question
- User can list all questions under a given speciality
- Admin can add a speciality

Ue the shared postman collection which has sample requests and response for the new endpoints. 

Make sure to call the `register` and `login` endpoints first to get a valid bearer token which you can use like so:
![image](https://github.com/Medic-Launch-Dev/MedicLaunchApi/assets/154400233/dcd1bf05-3490-46bd-8828-6b20ae4a5f9b)

Requests folder:
![image](https://github.com/Medic-Launch-Dev/MedicLaunchApi/assets/154400233/9e9d05b5-7776-483e-85fa-dd995f4347c3)


### Before Running The Project
Ensure that the following Environment Variables are set:
- `STRIPE_API_KEY`
- `STRIPE_PUBLISHABLE_KEY`
- `STRIPE_WEBHOOK_SECRET`
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_KEY`

If on mac, since you can't run localdb, set the `ConnectionStrings__DefaultConnection` environment variable to the connection string to `mediclaunchdevdb`


### Commonly Used `dotnet ef migrations` Commands

- **Add Migration**
  ```sh
  dotnet ef migrations add <MigrationName>
  ```

- **Remove Last Migration**
  ```sh
  dotnet ef migrations remove
  ```

- **Update Database**
  ```sh
  dotnet ef database update
  ```

- **Update Database to Specific Migration**
  ```sh
  dotnet ef database update <MigrationName>
  ```

- **List Migrations**
  ```sh
  dotnet ef migrations list
  ```

- **Generate SQL Script for Latest Migration**
  ```sh
  dotnet ef migrations script
  ```

- **Generate SQL Script for All Migrations**
  ```sh
  dotnet ef migrations script --from 0
  ```

- **Generate SQL Script for Specific Migrations**
  ```sh
  dotnet ef migrations script <FromMigration> <ToMigration>
  ```

- **Revert Database to Initial State**
  ```sh
  dotnet ef database update 0
  ```

- **Revert Database to previous migration**
  ```sh
  dotnet ef database update <PreviousMigrationName>
  ```
