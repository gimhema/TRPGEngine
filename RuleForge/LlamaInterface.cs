
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace RuleForge
{
    public sealed class ModelDescription : IDisposable
    {
        public string ModelPath { get; }
        public ModelParams ModelParams { get; }
        public LLamaWeights Model { get; }
        public LLamaContext ModelContext { get; }
        public InteractiveExecutor ModelExecutor { get; }
        public ChatHistory ChatHistory { get; }
        public ChatSession ChatSession { get; }
        public InferenceParams InferenceParams { get; }

        public ModelDescription(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentException("GGUF 모델 경로가 비어있습니다.", nameof(modelPath));

            var fullPath = Path.GetFullPath(modelPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("GGUF 모델 파일을 찾을 수 없습니다.", fullPath);

            ModelPath = fullPath;

            ModelParams = new ModelParams(ModelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = 0
            };

            Model = LLamaWeights.LoadFromFile(ModelParams);
            ModelContext = Model.CreateContext(ModelParams);
            ModelExecutor = new InteractiveExecutor(ModelContext);

            ChatHistory = new ChatHistory();
            ChatHistory.AddMessage(AuthorRole.System,
                "You are a helpful assistant. Answer in Korean. Keep responses concise.");

            ChatSession = new ChatSession(ModelExecutor, ChatHistory);

            InferenceParams = new InferenceParams
            {
                MaxTokens = 256,
                // "User:"는 Gemma 계열 템플릿과 안 맞을 수 있어요.
                // 일단 제거하거나, 모델에 맞는 stop을 찾는 쪽이 안전합니다.
                AntiPrompts = new List<string>(),
                SamplingPipeline = new DefaultSamplingPipeline(),
            };
        }

        public void Dispose()
        {
            // 생성한 역순으로 정리
            ModelContext?.Dispose();
            Model?.Dispose();
        }
    }

    public sealed class LlamaEngine : IDisposable
    {
        private readonly ModelDescription _model;

        public LlamaEngine(string modelPath)
        {
            _model = new ModelDescription(modelPath);
        }

        public async Task<string> ChatOnceAsync(string userText)
        {
            var parts = new List<string>();

            await foreach (var token in _model.ChatSession.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, userText),
                               _model.InferenceParams))
            {
                parts.Add(token);
            }

            return string.Concat(parts);
        }

        public void Dispose() => _model.Dispose();
    }
}
