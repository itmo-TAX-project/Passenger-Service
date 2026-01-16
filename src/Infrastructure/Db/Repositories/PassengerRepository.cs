using Application.Models;
using Application.Persistence;
using Application.Persistence.SearchFilters;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Infrastructure.Db.Repositories;

public class PassengerRepository(NpgsqlDataSource dataSource) : IPassengerRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<bool> AddPassengerAsync(Passenger passenger, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into passengers (passenger_name, passenger_phone)
                           values (:name, :phone)
                           on conflict(passenger_id) do nothing;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Boolean) { Value = passenger.Name });
        command.Parameters.Add(new NpgsqlParameter("phone", NpgsqlDbType.Boolean) { Value = passenger.Phone });

        int result = await command.ExecuteNonQueryAsync(cancellationToken);

        // Если пользователь добавлен, возвращает true, в противном случае произошел конфликт первичных ключей
        return result > 0;
    }

    public async Task UpdatePassengerAsync(Passenger passenger, CancellationToken cancellationToken)
    {
        const string sql = """
                           update passengers set
                           passenger_name=:name,
                           passenger_phone=:phone
                           where (passenger_id=:passenger_id);
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("passenger_id", NpgsqlDbType.Text) { Value = passenger.PassengerId });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Boolean) { Value = passenger.Name });
        command.Parameters.Add(new NpgsqlParameter("phone", NpgsqlDbType.Boolean) { Value = passenger.Phone });

        await command.ExecuteNonQueryAsync(cancellationToken);

        await UpdatePassengerPreferencesAsync(passenger, cancellationToken);
    }

    public async Task<Passenger?> GetPassengerBySearchFilterAsync(PassengerSearchFilter passengerSearchFilter, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        const string sql = """
                           select * from passengers
                           where (:id is null or passenger_id=:id)
                           and (:name is null or passenger_name=:name)
                           and (:phone is null or passenger_phone=:phone)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("passenger_id", NpgsqlDbType.Bigint)
        {
            Value = passengerSearchFilter.Id.HasValue ? passengerSearchFilter.Id.Value : DBNull.Value,
        });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text)
        {
            Value = passengerSearchFilter.Name != null ? passengerSearchFilter.Name : DBNull.Value,
        });
        command.Parameters.Add(new NpgsqlParameter("phone", NpgsqlDbType.Text)
        {
            Value = passengerSearchFilter.Phone != null ? passengerSearchFilter.Phone : DBNull.Value,
        });

        command.Parameters.Add(new NpgsqlParameter("cursor", NpgsqlDbType.Bigint) { Value = paginatedRequest.PageToken ?? 0 });
        command.Parameters.Add(new NpgsqlParameter("page_size", NpgsqlDbType.Integer) { Value = paginatedRequest.PageSize ?? 20 });

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        long id = reader.GetInt64(0);
        AllowedSegments? segments = await GetPassengerPreferencesByIdAsync(id, cancellationToken);
        return new Passenger(reader.GetName(1), reader.GetString(2), segments ?? throw new NoNullAllowedException(), id);
    }

    public async Task<PassengerPaginatedResponse> GetPassengersByPreferenceSearchFilterAsync(PreferenceSearchFilter preferenceSearchFilter, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        const string sql = """
                           select * from passengers p
                           join passenger_preferences pp on p.passenger_id = pp.passenger_id
                           where (:basic is null or basic_allowed=:basic)
                           and (:mid is null or mid_allowed=:mid)
                           and (:premium is null or premium_allowed=:premium)
                           and passenger_id > :cursor
                           order by passenger_id";"
                           limit :page_size
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("basic", NpgsqlDbType.Boolean)
        {
            Value = preferenceSearchFilter.BasicAllowed.HasValue ? preferenceSearchFilter.BasicAllowed.Value : DBNull.Value,
        });
        command.Parameters.Add(new NpgsqlParameter("mid", NpgsqlDbType.Boolean)
        {
            Value = preferenceSearchFilter.MidAllowed.HasValue ? preferenceSearchFilter.MidAllowed.Value : DBNull.Value,
        });
        command.Parameters.Add(new NpgsqlParameter("premium", NpgsqlDbType.Boolean)
        {
            Value = preferenceSearchFilter.PremiumAllowed.HasValue ? preferenceSearchFilter.PremiumAllowed.Value : DBNull.Value,
        });

        command.Parameters.Add(new NpgsqlParameter("cursor", NpgsqlDbType.Bigint) { Value = paginatedRequest.PageToken ?? 0 });
        command.Parameters.Add(new NpgsqlParameter("page_size", NpgsqlDbType.Integer) { Value = paginatedRequest.PageSize ?? 20 });

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var passengers = new List<Passenger>();
        long? lastId = paginatedRequest.PageToken;

        while (await reader.ReadAsync(cancellationToken))
        {
            bool basic = reader.GetBoolean(3);
            bool mid = reader.GetBoolean(4);
            bool premium = reader.GetBoolean(5);
            long passengerId = reader.GetInt64(0);

            var segments = new AllowedSegments(basic, mid, premium);
            passengers.Add(new Passenger(
                reader.GetString(1),
                reader.GetString(2),
                segments,
                passengerId));

            lastId = passengerId;
        }

        return new PassengerPaginatedResponse(passengers, lastId);
    }

    public async Task<AllowedSegments?> GetPassengerPreferencesByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = """
                           select * from passenger_preferences
                           where (:id = passenger_id)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint)
        {
            Value = id,
        });

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);
        return new AllowedSegments(
            reader.GetBoolean(0),
            reader.GetBoolean(1),
            reader.GetBoolean(2));
    }

    internal async Task<bool> AddPassengerPreferencesAsync(Passenger passenger, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into passenger_preferences (passenger_id, basic_allowed, mid_allowed, premium_allowed)
                           values (:passenger_id, :basic, :mid, :premium)
                           on conflict(passenger_id) do nothing;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("passenger_id", NpgsqlDbType.Text) { Value = passenger.PassengerId });
        command.Parameters.Add(new NpgsqlParameter("basic", NpgsqlDbType.Boolean) { Value = passenger.Segments.Basic });
        command.Parameters.Add(new NpgsqlParameter("mid", NpgsqlDbType.Boolean) { Value = passenger.Segments.Mid });
        command.Parameters.Add(new NpgsqlParameter("premium", NpgsqlDbType.Boolean) { Value = passenger.Segments.Premium });

        int result = await command.ExecuteNonQueryAsync(cancellationToken);

        // Если пользователь добавлен, возвращает true, в противном случае произошел конфликт первичных ключей
        return result > 0;
    }

    internal async Task UpdatePassengerPreferencesAsync(Passenger passenger, CancellationToken cancellationToken)
    {
        const string sql = """
                           update passenger_preferences set
                           basic_allowed=:basic,
                           mid_allowed=:mid,
                           premium_allowed=:premium
                           where (passenger_id=:passenger_id);
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("passenger_id", NpgsqlDbType.Text) { Value = passenger.PassengerId });
        command.Parameters.Add(new NpgsqlParameter("basic", NpgsqlDbType.Boolean) { Value = passenger.Segments.Basic });
        command.Parameters.Add(new NpgsqlParameter("mid", NpgsqlDbType.Boolean) { Value = passenger.Segments.Mid });
        command.Parameters.Add(new NpgsqlParameter("premium", NpgsqlDbType.Boolean) { Value = passenger.Segments.Premium });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}