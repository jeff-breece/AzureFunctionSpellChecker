# SpellCheckService
This Azure Function takes a string input from an HTTP Get request and passes this to Bing Spell Check in "Proof" mode, parses the response &amp; returns the suggested correction from the service.

## Settings
Add a local.settings.json file in the project root with the following configurations (your Market may be different and the valid modes are 'proof' & 'spell' depending on what you need).':
- "SubscriptionKey": "YOUR-BING-SPELLCHECK-KEY""
- "Endpoint": "https://api.bing.microsoft.com",
- "Path": "/v7.0/spellcheck?",
- "Market": "en-US",
- "Mode": "proof"

## Notes
- Some of this code was upcycled from other's works out on the net.

## References
- [Microsoft Documentation for Bing Search Services](https://docs.microsoft.com/en-us/azure/cognitive-services/bing-web-search/ )
- [API Priing](https://www.microsoft.com/en-us/bing/apis/pricing)
- [C# Quickstart](https://docs.microsoft.com/en-us/azure/cognitive-services/bing-spell-check/quickstarts/csharp)
- [Great video via PluralSight - must be a member to watch.](https://app.pluralsight.com/library/courses/microsoft-azure-cognitive-services-bing-spell-check-api/table-of-contents)
