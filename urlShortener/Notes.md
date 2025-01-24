# Notes
- use `Results` to return responses
- use `System.Text.Json.Serialization` and annotate a *record type field* with `[property: JsonPropertyName("custom_name")]` to set a custom json field name. annotate with `[JsonPropertyName("custom_name")]` if it is a class property
- Data annotations cannot be used directly with positional syntax for record types because Data Annotations apply only to properties, so must use the expanded syntax for records. Alternatively, can validate the fields manually or using FluentValidation library

## REST Client
- declare variables as `@host=http://localhost:8080`
### Sample Requests
```bash
@host=http://localhost:8080

GET {{host}}/test
Content-Type: application/json

###

POST {{host}}/shorten
Content-Type: application/json

{
    "sample": "data"
}

```