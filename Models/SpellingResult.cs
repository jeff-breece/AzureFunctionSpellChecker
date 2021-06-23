using System;
using System.Collections.Generic;
using System.Text;

namespace SpellCheckService.Models
{
    public class SpellingResult
    {
        public string ClientId { get; set; }

        public string TraceId { get; set; }

        public SpellingResponseBody Text { get; set; }

        public string CorrectedText { get; set; }
    }
}
