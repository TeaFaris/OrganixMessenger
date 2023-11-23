namespace OrganixMessenger.Documentation.ReDoc
{
    public static class RequestCodeGenerator
    {
        public static string GetCodeSample(
                ProgrammingLanguage language,
                Uri requestURL,
                OpenApiOperationDescription description,
                JToken? payloadSchema
            )
        {
            return language switch
            {
                var lang
                    when lang == ProgrammingLanguage.cURL => GeneratecURL(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.CSharp => GenerateCSharp(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.Python => GeneratePython(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.Go => GenerateGo(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.Java => GenerateJava(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.PHP => GeneratePHP(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.JavaScript => GenerateJavaScript(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.TypeScript => GenerateTypeScript(requestURL, description, payloadSchema),
                var lang
                    when lang == ProgrammingLanguage.Rust => GenerateRust(requestURL, description, payloadSchema),
                _ => language.DisplayName + " soon."
            };
        }

        private static string GeneratecURL(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var dataRaw = payloadSchema is not null
                            ? $"""
                              
                              -H 'Content-Type: application/json' \
                              -d '{payloadSchema}'
                              """
                            : string.Empty;

            return $$"""
                   curl -i -X {{description.Method.ToUpper()}} \
                   '{{requestURL}}' \
                   -H 'Authorization: Bearer token' \ {{dataRaw}}
                   """;
        }

        private static string GenerateCSharp(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var method = string.Concat(new ReadOnlySpan<char>(char.ToUpper(description.Method[0])), description.Method.AsSpan(1));

            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  var payload = new StringContent(
                                     @"
                                      {{payloadSchema
                                                .ToString()
                                                .Replace("\"", "\"\"")
                                                .Replace("\n", "\n    ")}}
                                      ",
                                      Encoding.UTF8,
                                      "application/json"
                                  );

                                  """
                                : string.Empty;

            var noPayloadCode = method is "Post" or "Put"
                                    ? ", null"
                                    : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? ", payload"
                                    : noPayloadCode;

            return $$"""
                   const string Token = "your token here";
                   const string RequestURL = "{{requestURL}}";

                   using var httpClient = new HttpClient();
                   httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {Token}");

                   // For client authorization
                   // httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
                   {{payloadCode}}
                   var response = await httpClient.{{method}}Async(RequestURL{{payloadArgCode}});
                   var responseString = await response.Content.ReadAsStringAsync();

                   Console.WriteLine("Status Code: " + response.StatusCode);
                   Console.WriteLine("Reason Phrase: " + response.ReasonPhrase);
                   Console.WriteLine("Response: " + responseString);
                   """;
        }

        private static string GeneratePython(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$""""

                                  payload = json.loads(\
                                  """
                                      {{payloadSchema
                                                .ToString()
                                                .Replace("\n", "\n    ")}}

                                  """)

                                  """"
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? ", json=payload"
                                    : string.Empty;

            var certificationVerificationArgCode =
#if DEBUG
                    ", verify=False";
#else
                    "";
#endif

            return $$"""
                   import requests

                   request_url = "{{requestURL}}"
                   token = "your token here"
                   headers = { "Authorization": f"Bot {token}" }
                   
                   # for client authorization
                   # headers = { "Authorization": f"Bearer {token}" }
                       {{payloadCode}}
                   response = requests.request("{{description.Method}}", request_url, headers=headers{{payloadSchema}}{{certificationVerificationArgCode}})
                   print("Status Code: ", response.status_code)
                   print("Response: ", response.text)
                   """;
        }

        private static string GenerateGo(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  jsonPayload := []byte(
                                  `
                                  {{payloadSchema.ToString()
                                                .Replace("\n", "\n ")}}
                                  `)

                                  """
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? ", bytes.NewBuffer(jsonPayload)"
                                    : string.Empty;

            return $$"""
                   package main

                   import (
                   	"bytes"
                   	"fmt"
                   	"net/http"
                   )

                   const Token = "your token here"
                   const RequestURL = "{{requestURL}}"

                   func main() {
                   	httpClient := &http.Client{}
                   	{{payloadCode}}
                   	req, err := http.NewRequest("{{description.Method.ToUpper()}}", RequestURL{{payloadArgCode}})
                   	if err != nil {
                   		fmt.Println("Error creating request:", err)
                   		return
                   	}

                   	req.Header.Set("Authorization", "Bot "+Token)

                    // For client authorization
                    // req.Header.Set("Authorization", "Bearer "+Token)

                   	req.Header.Set("Content-Type", "application/json")

                   	response, err := httpClient.Do(req)
                   	if err != nil {
                   		fmt.Println("Error sending request:", err)
                   		return
                   	}
                   	defer response.Body.Close()

                   	responseBody := new(bytes.Buffer)
                   	_, err = responseBody.ReadFrom(response.Body)
                   	if err != nil {
                   		fmt.Println("Error reading response body:", err)
                   		return
                   	}

                   	fmt.Println("Status Code: ", response.Status)
                   	fmt.Println("Response: ", responseBody.String())
                   }
                   """;
        }

        private static string GenerateJava(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadAppendCode = payloadSchema?
                                        .ToString()
                                        .Replace("\"", "\\\"")
                                        .Split(Environment.NewLine)
                                        .Select(x => string.Concat(".append(\"", x + @"\n"")"));

            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  String jsonPayload = new StringBuilder()
                                  {{string.Join('\n', payloadAppendCode!)}}
                                  .toString();

                                  """
                                : string.Empty;

            var noPayloadCode = description.Method is "post" or "put"
                                    ? "HttpRequest.BodyPublishers.noBody()"
                                    : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? "HttpRequest.BodyPublishers.ofString(jsonPayload, StandardCharsets.UTF_8)"
                                    : noPayloadCode;

            return $$""""
                   String token = "your token here";
                   String requestURL = "{{requestURL}}";

                   HttpClient httpClient = HttpClient.newBuilder().build();
                   {{payloadCode}}
                   HttpRequest request = HttpRequest.newBuilder()
                           .uri(URI.create(requestURL))
                           .header("Authorization", "Bot " + token)
                   //       For client authorization
                   //      .header("Authorization", "Bearer " + token)
                           .header("Content-Type", "application/json")
                           .{{description.Method.ToUpper()}}({{payloadArgCode}})
                           .build();

                   HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());

                   System.out.println("Status Code: " + response.statusCode());
                   System.out.println("Reason Phrase: " + response.reasonPhrase());
                   System.out.println("Response: " + response.body());
                   """";
        }

        private static string GeneratePHP(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  $jsonPayload = 
                                  '
                                  {{payloadSchema}}
                                  ';

                                  """
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? "curl_setopt($ch, CURLOPT_POSTFIELDS, $jsonPayload);"
                                    : string.Empty;

            return $$"""
                   $token = "your token here";
                   $requestURL = "{{requestURL}}";
                   {{payloadCode}}
                   $headers = array(
                       'Authorization: Bot ' . $token,
                   //   For client authorization
                   //  'Authorization: Bearer ' . $token,
                       'Content-Type: application/json',
                   );

                   $ch = curl_init($requestURL);

                   curl_setopt($ch, CURLOPT_CUSTOMREQUEST, '{{description.Method.ToUpper()}}');
                   curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
                   {{payloadArgCode}}
                   curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

                   $response = curl_exec($ch);

                   if (curl_errno($ch)) {
                       echo 'Error: ' . curl_error($ch);
                   }

                   curl_close($ch);

                   echo "Response: " . $response;
                   """;
        }

        private static string GenerateJavaScript(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  const payload = {{payloadSchema}};

                                  """
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? ",\n  body: JSON.stringify(payload)"
                                    : string.Empty;

            return $$"""
                   const token = "your token here";
                   const requestURL = "{{requestURL}}";
                   {{payloadCode}}
                   const headers = new Headers();
                   headers.append("Authorization", "Bot " + token);
                   // For client authorization
                   // headers.append("Authorization", "Bearer " + token);
                   headers.append("Content-Type", "application/json");

                   const requestOptions = {
                     method: "{{description.Method.ToUpper()}}",
                     headers: headers{{payloadArgCode}}
                   };

                   fetch(requestURL, requestOptions)
                     .then(response => response.text())
                     .then(data => {
                       console.log("Response:", data);
                     })
                     .catch(error => {
                       console.error("Error:", error);
                     });
                   """;
        }

        private static string GenerateTypeScript(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$"""

                                  const payload = {{payloadSchema}};

                                  """
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? ", payload"
                                    : string.Empty;

            return $$"""
                   import axios, { AxiosRequestConfig } from 'axios';

                   const Token: string = "your token here";
                   const RequestURL: string = "{{requestURL}}";

                   const headers: AxiosRequestConfig = {
                     headers: {
                       'Authorization': `Bot ${Token}`,
                   //   For client authorization
                   //  'Authorization': `Bearer ${Token}`,
                     }
                   };
                   {{payloadCode}}
                   axios.{{description.Method}}(RequestURL{{payloadArgCode}}, headers)
                     .then((response) => {
                       console.log("Status Code: " + response.status);
                       console.log("Response: " + JSON.stringify(response.data));
                     })
                     .catch((error) => {
                       console.error("Error: " + error);
                     });
                   """;
        }

        private static string GenerateRust(Uri requestURL, OpenApiOperationDescription description, JToken? payloadSchema)
        {
            var payloadCode = payloadSchema is not null
                                ? $$"""
                                                                    
                                  let payload =
                                  r#"
                                  {{payloadSchema}}
                                  "#;
                                  
                                  """
                                : string.Empty;

            var payloadArgCode = payloadSchema is not null
                                    ? "\n.body(payload)"
                                    : string.Empty;

            return $$"""
                   use reqwest::header::HeaderValue;
                   use reqwest::Client;

                   #[tokio::main]
                   async fn main() -> Result<(), Box<dyn std::error::Error>> {
                       const TOKEN: &str = "your token here";
                       const REQUEST_URL: &str = "{{requestURL}}";

                       let client = Client::new();

                       let mut headers = reqwest::header::HeaderMap::new();
                       headers.insert(
                           "Authorization",
                           HeaderValue::from_str(&format!("Bot {}", TOKEN))?,
                       );

                       // For client authorization
                       // headers.insert(
                       //     "Authorization",
                       //     HeaderValue::from_str(&format!("Bearer {}", TOKEN))?,
                       // );
                       {{payloadCode}}
                       let response = client
                           .{{description.Method}}(REQUEST_URL)
                           .headers(headers){{payloadArgCode}}
                           .send()
                           .await?;

                       let response_text = response.text().await?;
                       println!("Status Code: {}", response.status());
                       println!("Reason Phrase: {:?}", response.status().canonical_reason());
                       println!("Response: {}", response_text);

                       Ok(())
                   }
                   """;
        }
    }
}
