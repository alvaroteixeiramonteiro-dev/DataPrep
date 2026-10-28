using DataPrep.Infrastructure.Csv;
using Microsoft.AspNetCore.Localization;

namespace DataPrep.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Recursos de tradução guardados na pasta Resources.
            builder.Services.AddLocalization(options =>
                options.ResourcesPath = "Resources");

            // O leitor não mantém estado entre operações.
            builder.Services.AddTransient<CsvDataReader>();

            // Verifica a estrutura do CSV e, futuramente, os campos mapeados.
            builder.Services.AddTransient<CsvValidationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // O idioma pode ser escolhido pelo parâmetro de URL "culture".
            // Novos idiomas podem ser acrescentados com os respetivos recursos.
            var supportedCultures = new[] { "pt-PT", "en" };

            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture("pt-PT")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures));

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
