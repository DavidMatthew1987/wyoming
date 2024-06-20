namespace AnalyticsWebApi.Datamodel;

public class AnalyticsData
{
    public int Id { get; set; }
    public string? DataKey { get; set; }
    public string? Data { get; set; }
    public string? Referer { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

//DROP TABLE IF EXISTS analytics;

//CREATE TABLE analytics(
//    id SERIAL PRIMARY KEY,
//    datakey TEXT, -- Can be null
//    data TEXT NOT NULL,
//    referer TEXT, -- Can be null
//    ipaddress TEXT,
//    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
//    CONSTRAINT unique_referer_ipaddress_datakey UNIQUE (referer, ipaddress, datakey)
//);
