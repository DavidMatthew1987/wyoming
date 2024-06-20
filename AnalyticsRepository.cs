using AnalyticsWebApi.Datamodel;
using Dapper;
using Npgsql;

namespace AnalyticsWebApi;

public class AnalyticsRepository(string connectionString)
{
    public async Task<int> SaveAnalyticsData(AnalyticsData analyticsData)
    {
        using NpgsqlConnection connection = new(connectionString);
        string checkSql = @"
        SELECT COUNT(*) FROM analytics 
        WHERE referer = @Referer AND ipaddress = @IpAddress AND datakey = @DataKey;";

        int exists = await connection.ExecuteScalarAsync<int>(checkSql, new { analyticsData.Referer, analyticsData.IpAddress, analyticsData.DataKey });

        if (exists > 0)
        {
            // Record already exists, update it
            string updateSql = @"
            UPDATE analytics
            SET data = @Data, ipaddress = @IpAddress, created_at = @CreatedAt
            WHERE referer = @Referer AND datakey = @DataKey AND ipaddress = @IpAddress;
        ";
            return await connection.ExecuteAsync(updateSql, new { analyticsData.Data, analyticsData.CreatedAt, analyticsData.Referer, analyticsData.IpAddress, analyticsData.DataKey });
        }
        else
        {
            // Insert a new record
            string insertSql = @"
            INSERT INTO analytics (datakey, data, referer, ipaddress, created_at)
            VALUES (@DataKey, @Data, @Referer, @IpAddress, @CreatedAt);
        ";
            return await connection.ExecuteAsync(insertSql, new { analyticsData.DataKey, analyticsData.Data, analyticsData.Referer, analyticsData.IpAddress, analyticsData.CreatedAt });
        }
    }

    public async Task<AnalyticsData?> GetAnalyticsDataByKey(string key, string referer, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            using NpgsqlConnection connection = new(connectionString);
            string sql = "SELECT id, datakey, data, referer, ipaddress, created_at CreatedAt FROM analytics WHERE datakey = @Key AND referer = @Referer;";
            return await connection.QueryFirstOrDefaultAsync<AnalyticsData>(sql, new { Key = key, Referer = referer });
        }
        else
        {
            using NpgsqlConnection connection = new(connectionString);
            string sql = "SELECT id, datakey, data, referer, ipaddress, created_at CreatedAt FROM analytics WHERE datakey = @Key AND referer = @Referer AND ipaddress = @IpAddress;";
            return await connection.QueryFirstOrDefaultAsync<AnalyticsData>(sql, new { Key = key, Referer = referer, IpAddress = ipAddress });
        }
    }

}