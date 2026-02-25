namespace Api;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

/// <summary>
/// Custom serializer used by Swashbuckle to return a pre-written OpenAPI document from disk.
/// This allows serving a contract-first YAML file instead of letting Swashbuckle generate the document.
/// </summary>
public class CustomSwaggerDocumentSerializer : ISwaggerDocumentSerializer
{
    /// <summary>
    /// Serializes the OpenAPI document by writing the contents of the local `openApi.yml` file directly
    /// to the provided writer. This replaces the auto-generated document with the contract-first file.
    /// </summary>
    /// <param name="document">The OpenApiDocument provided by Swashbuckle (ignored by this implementation).</param>
    /// <param name="writer">The writer to which the YAML content should be written.</param>
    /// <param name="specVersion">The OpenAPI specification version (ignored).</param>
    public void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteRaw(File.ReadAllText("openApi.yml"));
    }
}
