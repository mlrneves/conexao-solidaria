using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Doacoes.Api.Infrastructure;

public static class MongoConfig
{
    private static bool _registrado;

    /// <summary>
    /// Configuração global do driver 3.x: Guid no formato Standard (obrigatório
    /// a partir do driver 3) e nomes de campos em camelCase no BSON.
    /// </summary>
    public static void Registrar()
    {
        if (_registrado) return;
        _registrado = true;

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        ConventionRegistry.Register(
            "camelCase",
            new ConventionPack { new CamelCaseElementNameConvention() },
            _ => true);
    }
}
