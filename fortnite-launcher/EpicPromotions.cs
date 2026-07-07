using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FortniteLauncher;

public record PromoGame(
    string Title,
    string? Description,
    bool IsCurrentlyFree,
    DateTime? FreeUntil,
    DateTime? FreeFrom,
    string? StoreUrl);

/// <summary>
/// Consulta el mismo endpoint publico que usa la tienda de Epic Games para
/// mostrar la seccion de "juegos gratis" (actual y proximo), sin necesidad
/// de sesion ni claves de API.
/// </summary>
public static class EpicPromotions
{
    private const string Endpoint =
        "https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions?locale=es-ES&country=ES&allowCountries=ES";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static async Task<List<PromoGame>> FetchCurrentAndUpcomingAsync()
    {
        var result = new List<PromoGame>();

        try
        {
            var json = await Http.GetStringAsync(Endpoint);
            using var doc = JsonDocument.Parse(json);

            var elements = doc.RootElement
                .GetProperty("data")
                .GetProperty("Catalog")
                .GetProperty("searchStore")
                .GetProperty("elements");

            foreach (var el in elements.EnumerateArray())
            {
                var title = el.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                var description = el.TryGetProperty("description", out var d) ? d.GetString() : null;

                string? slug = el.TryGetProperty("productSlug", out var ps) && ps.ValueKind == JsonValueKind.String
                    ? ps.GetString()
                    : el.TryGetProperty("urlSlug", out var us) ? us.GetString() : null;
                string? storeUrl = string.IsNullOrWhiteSpace(slug)
                    ? null
                    : $"https://store.epicgames.com/es-ES/p/{slug}";

                DateTime? freeUntil = null;
                DateTime? freeFrom = null;
                var isFree = false;

                if (el.TryGetProperty("promotions", out var promos) && promos.ValueKind == JsonValueKind.Object)
                {
                    if (promos.TryGetProperty("promotionalOffers", out var current) && current.GetArrayLength() > 0)
                    {
                        var offer = current[0].GetProperty("promotionalOffers")[0];
                        if (offer.TryGetProperty("endDate", out var end) && end.TryGetDateTime(out var endDate))
                        {
                            freeUntil = endDate;
                        }
                        isFree = true;
                    }
                    else if (promos.TryGetProperty("upcomingPromotionalOffers", out var upcoming) && upcoming.GetArrayLength() > 0)
                    {
                        var offer = upcoming[0].GetProperty("promotionalOffers")[0];
                        if (offer.TryGetProperty("startDate", out var start) && start.TryGetDateTime(out var startDate))
                        {
                            freeFrom = startDate;
                        }
                    }
                }

                if (isFree || freeFrom is not null)
                {
                    result.Add(new PromoGame(title, description, isFree, freeUntil, freeFrom, storeUrl));
                }
            }
        }
        catch
        {
            // Sin conexion o cambio en la API publica de Epic: se devuelve lista vacia.
        }

        return result;
    }
}
