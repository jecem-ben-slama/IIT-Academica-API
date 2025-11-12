
using IIT_Academica_API.Data;
using Microsoft.EntityFrameworkCore;

namespace IIT_Academica_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Add services to the container.
            var cnx = builder.Configuration.GetConnectionString("cnx");

            // 2. Register the ApplicationDbContext service
            builder.Services.AddDbContext<ApplicationDbContext>(options =>options.UseSqlServer(cnx));
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
