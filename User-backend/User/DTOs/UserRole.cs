using System.Text.Json.Serialization;

namespace User.DTOs
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Manager,
        Employee,
        Intern
    }

}
