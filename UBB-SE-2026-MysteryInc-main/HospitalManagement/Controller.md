## Of sugar spicy and everything nice (Controllers)

0. Inside of the Common.API -> appsettings.json change the connection string to your db connection string;

1. Services should be moved inside of the Common.API project.

2. Inside the "Controllers" folder you have to create the respective Controller class for the service.

3. Look at the example inside of AllergyController.cs for an example of this.

4. Add your moved service and repository from Common.Data inside of the Program.cs, DO NOT MOVE THE REPOSITORY FROM THE COMMON.
Data INTO Common.API   ->  Look for the example inside of the Program.cs for loading. If such services are also loaded inside of your App.xaml.cs TAKE THEM OUT

5. In your project with the views and view models. Create a folder called Proxy where all files are inheriting from ProxyBase.cs Do not recreate the folder if it exists already...

6. Inside that folder make another folder with the name of your proxy (the class that links to your controller) and create it and its interface. Look inside Proxy/AllergyProxy for an example.

7. Add the newly created project to the services collection in your App.xaml.cs. Here is an example.
```
_ = services.AddHttpClient<IAllergyProxy, AllergyProxy>((client) =>
{
    var uriString = AppConfiguration["ApiSettings:BaseUri"];

    if (string.IsNullOrEmpty(uriString))
    {
        throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
    }

    client.BaseAddress = new Uri(uriString);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```
8. Look inside your appsettings.local.json to contain the new fields from the appsettings.example.json ApiSettings:BaseUri. You dont need to change its value.

9. Change your ViewModel to get the data via the Proxy not the previous service.

10. See if it works!