using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace UserProfileService.Utilities;

internal class TextPlainInputFormatter : TextInputFormatter
{
    public TextPlainInputFormatter()
    {
        SupportedMediaTypes.Add("text/plain");
        SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        SupportedEncodings.Add(UTF16EncodingLittleEndian);
    }

    protected override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(
        InputFormatterContext context,
        Encoding encoding)
    {
        if (context.HttpContext.Request.Body == null)
        {
            throw new ValidationException("Body of request cannot be null!");
        }

        using var streamReader = new StreamReader(context.HttpContext.Request.Body, encoding);
        string data = await streamReader.ReadToEndAsync();

        return await InputFormatterResult.SuccessAsync(data);
    }
}
