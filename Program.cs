using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

public class User 
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public List<Message> Messages { get; set; }
}

public class Message
{
    [Key]
    public int id { get; set; }

    [Required]
    public string Value { get; set; }

    [Required]
    public DateTime SentAt { get; set; }
}

public class ExperimentDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Connect to underlying DB
        var connStr = new SqliteConnectionStringBuilder()
        {
            DataSource = "mydb.db",
        }.ToString();
        builder.UseSqlite(connStr);
        builder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasMany(u => u.Messages);
    }
}

public static class Program 
{
    public static void Main(string[] args)
    {
        var db = new ExperimentDbContext();
        db.Database.EnsureCreated();

        for (;;)
        {
            // Imprimir menu
            Console.Clear();
            Console.WriteLine("Seleccione una opción: ");
            Console.WriteLine("1. Entrar al chat");
            Console.WriteLine("2. Salir");

            // Leer decisión del sdtin y actuar en consecuencia
            var option = Console.ReadLine();
            if (option == "1") {
                // Imprimir menu
                Console.Clear();
                Console.WriteLine("Introduzca nombre de usuario: ");
                
                // Leer datos
                var username = Console.ReadLine();

                // Buscar usuario existente o crear uno nuevo
                var user = db.Users.FirstOrDefault<User>(u => u.Name == username);
                if (user == null) {
                    user = new User { Name = username, Messages = new() };
                    db.Users.Add(user);
                    db.SaveChanges();
                }

                // Bucle del prompt
                for (;;)
                {   
                    // Imprimir los mensajes de todos los usuarios
                    var messages = db.Messages
                        .OrderBy(m => m.SentAt)
                        .ToList();
                    Console.Clear();
                    foreach (var msg in messages)
                    {
                        var msgUser = db.Users.FirstOrDefault(u => u.Messages.Contains(msg));
                        Console.WriteLine($"{msgUser.Name}> {msg.Value}");
                    }

                    // Prompt donde el usuario escribe mensajes nuevos
                    Console.Write(" > ");
                    var value = Console.ReadLine();
                    if (value == ":exit")
                        break;

                    // Crear y guardar mensaje
                    var refUser = db.Users.First(u => u.Name == username);
                    var newMessage = new Message() {
                        Value = value,
                        SentAt = DateTime.UtcNow,
                    };
                    refUser.Messages.Add(newMessage);
                    db.Messages.Add(newMessage);
                    db.SaveChanges();
                }
            } else {
                break;
            }
        }
    }
}
