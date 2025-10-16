using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.Text;

namespace SemanticDocIngestor.Infrastructure.Factories.Ollama
{
    public static class RagPromptBuilder
    {
        private const string RagPromptTemplate = """
                    You are a document analysis AI assistant. Answer strictly and only using the text within the provided document chunks. Do not use external knowledge or make assumptions.

                    ANSWERING POLICY:
                    - Carefully review ALL chunks for relevant information, including paraphrases and synonyms.
                    - Prioritize chunks with direct relevance to the question.
                    - If any chunk contains directly relevant information, answer concisely using that information.
                    - If multiple chunks provide relevant information, synthesize a comprehensive answer.
                    - Do not reference chunk numbers or metadata in your answer.
                    - If no chunks are relevant, respond with "The provided document chunks do not contain information relevant to the question." in the same language as the question
                    - Don't copy text verbatim; synthesize a concise answer.
                    - If multiple chunks provide relevant but conflicting information, note the discrepancy and summarize the differing viewpoints.
                    - Always answer in the same language as the question.
                    - If the question is vague or ambiguous, answer based on the most likely interpretation supported by the chunks.
                    - If the question is unrelated to the document chunks, respond with "The provided document chunks do not contain information relevant to the question." in the same language as the question.
                    - Use proper grammar and spelling.
                    - If the question is a yes/no question and the answer is contained in the chunks, respond with "Yes" or "No" only.

                    Document Chunks:
                    {{$context}}

                    Question: {{$question}}
                    """;

        // New: Template for LLM-driven chunking of raw document content into string chunks
        private const string ChunkingPromptTemplate = """
                    You are a document chunking assistant. Split the provided document content into coherent, self-contained string chunks.

                    CHUNKING RULES:
                    - Max characters per chunk: {{$max_chars}}
                    - Overlap characters between consecutive chunks: {{$overlap_chars}}
                    - Prefer sentence boundaries: {{$prefer_sentence_boundaries}}
                    - Do not omit any content or summarize. Preserve original order and meaning.
                    - If a sentence exceeds the limit, split at clause/word boundaries without breaking a word.
                    - Merge small sentences where needed to approach the target size without exceeding it.
                    - Normalize whitespace: {{$normalize_whitespace}} (collapse repeated spaces/newlines if true).
                    - Trim leading/trailing whitespace in each chunk: {{$trim_whitespace}}.
                    - Keep special characters and punctuation from the source text.
                    - Ensure each chunk is a valid UTF-8 string.
                    - Return chunks in their original order.
                    - If the input text is empty or null, return an empty array.
                    - Respond ONLY with a JSON array of strings representing the chunks.

                    OUTPUT FORMAT:
                    - Return ONLY a JSON array of strings.
                    - Do not include any commentary, code fences, or additional keys.
                    - Example: ["chunk 1 text", "chunk 2 text", "..."]

                    DOCUMENT CONTENT:
                    {{$text}}
                    """;

        // New: Options for building a chunking prompt
        public sealed class ChunkingPromptOptions
        {
            public int MaxCharactersPerChunk { get; init; } = 1000;
            public int OverlapCharacters { get; init; } = 200; // ~20% of default 
            public bool PreferSentenceBoundaries { get; init; } = true;
            public bool NormalizeWhitespace { get; init; } = true;
            public bool TrimWhitespace { get; init; } = true;
        }

        public static string Build(List<DocumentChunkDto> contextChunks, string question)
        {
            if (contextChunks == null || contextChunks.Count == 0)
            {
                return RagPromptTemplate
                    .Replace("{{$context}}", "No document chunks available for analysis.")
                    .Replace("{{$question}}", question?.Trim() ?? string.Empty);
            }

            var contextBuilder = new StringBuilder();

            for (int i = 0; i < contextChunks.Count; i++)
            {
                var chunk = contextChunks[i];
                if (chunk?.Content == null) continue;

                var chunkNumber = i + 1;
                var metadata = chunk.Metadata;

                // Add chunk header with metadata including file path (to enable path-aware citations)
                contextBuilder.AppendLine($"--- Document Chunk {chunkNumber} ---");
                if (metadata != null)
                {
                    contextBuilder.AppendLine($"Source: {metadata.FileName ?? "Unknown"}" +
                                              (metadata.PageNumber != null ? $" (Page {metadata.PageNumber})" : "") +
                                              (metadata.SectionTitle != null ? $" - {metadata.SectionTitle}" : "") +
                                              (string.IsNullOrEmpty(metadata.FilePath) == false ? $" - FilePath: {metadata.FilePath}" : ""));
                }

                // Add content
                contextBuilder.AppendLine(chunk.Content.Trim());
                contextBuilder.AppendLine();
            }

            var context = contextBuilder.ToString().TrimEnd();
            var prompt = RagPromptTemplate
                .Replace("{{$context}}", context)
                .Replace("{{$question}}", question?.Trim() ?? string.Empty);

            return prompt;
        }

        // Build a RAG prompt directly from plain string chunks (no metadata)
        public static string BuildFromStringChunks(List<string> contextChunks, string question)
        {
            if (contextChunks == null || contextChunks.Count == 0)
            {
                return RagPromptTemplate
                    .Replace("{{$context}}", "No document chunks available for analysis.")
                    .Replace("{{$question}}", question?.Trim() ?? string.Empty);
            }

            var contextBuilder = new StringBuilder();
            for (int i = 0; i < contextChunks.Count; i++)
            {
                var content = contextChunks[i];
                if (string.IsNullOrWhiteSpace(content)) continue;

                var chunkNumber = i + 1;
                contextBuilder.AppendLine($"--- Document Chunk {chunkNumber} ---");
                contextBuilder.AppendLine(content.Trim());
                contextBuilder.AppendLine();
            }

            var context = contextBuilder.ToString().TrimEnd();
            return RagPromptTemplate
                .Replace("{{$context}}", context)
                .Replace("{{$question}}", question?.Trim() ?? string.Empty);
        }

        // Build a chunking prompt to instruct an LLM to split raw text into proper string chunks
        public static string BuildChunkingPrompt(string text, ChunkingPromptOptions? options = null)
        {
            options ??= new ChunkingPromptOptions();

            var prompt = ChunkingPromptTemplate
                .Replace("{{$max_chars}}", options.MaxCharactersPerChunk.ToString())
                .Replace("{{$overlap_chars}}", options.OverlapCharacters.ToString())
                .Replace("{{$prefer_sentence_boundaries}}", options.PreferSentenceBoundaries ? "true" : "false")
                .Replace("{{$normalize_whitespace}}", options.NormalizeWhitespace ? "true" : "false")
                .Replace("{{$trim_whitespace}}", options.TrimWhitespace ? "true" : "false")
                .Replace("{{$text}}", text ?? string.Empty);

            return prompt;
        }
    }
}