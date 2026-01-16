namespace Application.Persistence.SearchFilters;

public record PreferenceSearchFilter(bool? BasicAllowed = null, bool? MidAllowed = null, bool? PremiumAllowed = null);