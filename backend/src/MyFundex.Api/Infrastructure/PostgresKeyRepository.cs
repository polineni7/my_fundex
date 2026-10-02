using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Npgsql;

namespace MyFundex.Api.Infrastructure;

// Data Protection's repository interface is synchronous; reads are cached by its key manager.
public sealed class PostgresKeyRepository(string connectionString) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("SELECT \"SettingValue\" FROM fundex_configuration.\"Settings\" WHERE \"SettingKey\" LIKE 'DataProtection.KeyRing.%' AND \"Category\" = 'DataProtection' AND \"Environment\" = 'GLOBAL' AND \"IsDeleted\" = false ORDER BY \"Id\"", connection);
        using var reader = command.ExecuteReader();
        var elements = new List<XElement>();
        while (reader.Read()) elements.Add(XElement.Parse(reader.GetString(0)));
        return elements.AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        // A key must already be certificate-wrapped by Data Protection before persistence.
        if (element.Name.LocalName == "key" && !element.Descendants().Any(x => x.Name.LocalName == "encryptedSecret"))
            throw new InvalidOperationException("Refusing to store an unencrypted Data Protection key.");
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("""
            INSERT INTO fundex_configuration."Settings"
            ("SettingId", "SettingKey", "SettingValue", "ValueType", "Category", "Environment", "IsSensitive", "CreatedAt", "CreatedBy", "IsDeleted", "Version")
            VALUES (@id, @name, @xml, 'EncryptedXml', 'DataProtection', 'GLOBAL', true, now(), 0, false, 1)
            """, connection);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("name", "DataProtection.KeyRing." + friendlyName);
        command.Parameters.AddWithValue("xml", element.ToString(SaveOptions.DisableFormatting));
        command.ExecuteNonQuery();
    }
}
