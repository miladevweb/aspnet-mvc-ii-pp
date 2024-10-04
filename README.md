<style>
    h1 {
        font-size: 3rem;
    }
    h3 {
        font-size: 1.5rem;
        font-weight: bold;
        text-decoration: underline;
    }
    imp::after {
        content: 'IMPORTANT!!!';
        color: hsl(150 100% 60% / 100%);
        font-size: 3.5rem;
        font-weight: 900;
        text-decoration: underline;
        display: block;
    }
</style>

# Create a Controller

Debemos Crear nuestro controller el cual indicará la ruta donde estará nuestra lógica de negocios o API, nuestro controller tiene que heredar la clase **CONTROLLER**. Cada metodo de la clase controller será llamado **ACTION** y será como una subruta de la ruta principal **CONTROLLER**.

Para que nuestra vista se reenderize, tenemos que retornar un View method en nuestra action y ademas hacer que la misma sea de tipo IActionResult, por lo general se recomienda que sea de tipo asincrona.

```cs
// Controllers/HelloWorldController.cs
public class HelloWorldController : Controller
{
    public IActionResult Index()
    {   // http://localhost:8000/HelloWorld
        return View();
    }
    public IActionResult Welcome(string name="DEFAULT_NAME", int ID = 10)
    {   // http://localhost:8000/HelloWorld/Welcome/20?name=another_name
        return View();
    }
}
```

<br>

Ahora tenemos que crear la Vista, para eso crearemos los archivos `Views/HelloWorld/Index.cshtml` y `Views/HelloWorld/Python.cshtml`.

<imp></imp>
Como vemos en la Action Welcome del Controller HelloWorld hay 2 parametros, name y ID, name será un Query Param y ID y Param porque en Program.cs definimos que todas las rutas tendrán como Param al parametro ID y que todos los demás que vengan despues de ID serán Query Param `?`

```cs
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
```

<br>

Podemos usar variables creadas en el controllador o desde la vista para representarlas dinamicamente en el template.

```cs
// Welcome Action
public IActionResult Welcome(string name="DEFAULT_NAME", int numTimes = 1, int ID = 10)
    {
        ViewData["NumTimes"] = numTimes;
        ViewData["Message"] = $"Hello {name}, your ID is {ID}";
        return View();
    }
```

```html
@{ ViewData["Name"] = "DEFAULT_NAME"; }

<h1>My name is: @ViewData["Name"]</h1>
<ul>
  @for(int i=0 ; i < (int)ViewData["NumTimes"]!; i++) {
  <li>@ViewData["Message"]</li>
  }
</ul>
```

<br>

# Crear un Modelo

Para representar los datos dinamicamente desde nuestra base de datos necesitamos un modelo que contenga toda nuestra logica de negocio `Database`, para eso necesitamos una base de datos.

### Create a Database with Docker

- Create a Docker Image

```bash
docker pull mcr.microsoft.com/mssql/server
```

<br>

- Create a Container

```bash
docker run --name mssql --rm -d -p 1433:1433 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=My@Password" -v "sqlvolume:/var/opt/mssql" mcr.microsoft.com/mssql/server
```

<br>

- Add Connection String in secrets or in `appsettings.json`

```json
"ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=MyDatabaseName;User ID=sa;Password=My@Password;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
```

<br>

### Instalar Entity Framework Core

- Luego en Program.cs establecer nuestra conexion necesitamos instalar EntityFrameworkCore

```bash
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.5
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.5
```

- Ahora debemos de crear el contexto de nuestra DB, nuestra tabla se llamará **_Movie_**

```cs
public class MvcMovieContext : DbContext
{
    public MvcMovieContext(DbContextOptions<MvcMovieContext> options) : base(options)
    {}
    public DbSet<Movie> Movie { get; set;}
}
```

- Nos vamos a `Program.cs` para establecer la conexion entre nuestra app con nuestra base de datos

```cs
// Add Entity Framework Core services
builder.Services.AddDbContext<MvcMovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

### Hacer migraciones y actualizar base de datos

Para que nuestra base de datos realice cambios necesitamos hacer migrations y para que esos cambios se vean reflejados necesitamos actualizar nuestra DB

```bash
dotnet ef migrations add <migration-name> --output-dir <path/to/migrations>
dotnet ef database update
```

### Injectar Contexto en el Controller

- Para usar nuestra DB necesitamos inyectar nuestro contexto en el controllador

```cs
public class MoviesController : Controller
{
    private readonly MvcMovieContext _context;
    public MoviesController(MvcMovieContext context)
    {
        _context = context;
    }
}
```

Ahora si ya podemos crear nuestras Query y Mutations en nuestro Controller!!!

<br>

# Razor Pages

Podemos usar nuestro modelo y sus valores en las vistas

```html
@model IEnumerable<MvcPractice.Models.Movie>
  @{ ViewData["Title"] = Index; }
  <th>@Html.DisplayNameFor(model => model.Title)</th>
  <td>@Html.DisplayFor(modelItem => item.Title)</td></MvcPractice.Models.Movie
>
```

**_DisplayNameFor_** muestra el nombre de la propiedad de nuestra DB y **_DisplayFor _** muestra su valor.

<br>

Muchas veces necesitamos usar datos de prueba para probar nuestra interfaz, para eso podemos usar el method OnModelCreating de nuestro DBContext para generarlos.

```cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>().HasData(
            new Movie()
            {
                Id = 1,
                Title = "The Shawshank Redemption",
                ReleaseDate = new DateTime(1994, 9, 10),
                Genre = "Drama",
                Price = 9.99M
            },
            new Movie()
            {
                Id = 2,
                Title = "The Godfather",
                ReleaseDate = new DateTime(1972, 3, 24),
                Genre = "Crime, Drama",
                Price = 14.99M
            }
        );
    }
```

Ahora necesitamos crear la migration y actualizar nuestra DB.

<br>

### ActionName - Action con el mismo nombre y parametros distintos - Action con el mismo name y HTTP methods distintos

Tanto el mismo action con parametros distintos, un action un action con el mismo nombre pero con HTTP methods distintos, o un action distinto con un ActionName son usados para evitar confusiones entre los actions que llevan el mismo nombre

```cs
    public async Task<IActionResult> Index(string movieGenre, string searchString)
    {
        if (_context.Movie == null)
        {
            return Problem("Entity set 'MvcMovieContext.Movie'  is null.");
        }
        // Use LINQ to get list of genres.
        IQueryable<string> genreQuery = from m in _context.Movie
                                        orderby m.Genre
                                        select m.Genre;
        IQueryable<Movie> movies = from m in _context.Movie
                                   select m;

        if (!string.IsNullOrEmpty(searchString))
        {
            movies = movies.Where(s => s.Title!.Contains(searchString));
        }
        if (!string.IsNullOrEmpty(movieGenre))
        {
            movies = movies.Where(x => x.Genre == movieGenre);
        }
        var movieGenreVM = new MovieGenreViewModel
        {
            // SelectList used to populate the dropdown list <select></select>
            Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
            Movies = await movies.ToListAsync()
        };
        // Pass the view model to the view, not necessarily the DB model
        return View(movieGenreVM);
    }

    // Incluso no es necesario usar el parametro notUsed ya que es un method HTTP distinto. El above es HTTPGet by default y este es HTTPPost.
    [HttpPost]
    public string Index(string searchString, bool notUsed)
    {
        return "From [HttpPost]Index: filter on " + searchString;
    }
```
