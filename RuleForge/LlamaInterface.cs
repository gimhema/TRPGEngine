
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

    /// <summary>NPC별 독립 채팅 세션. LLamaWeights를 공유하고 LLamaContext는 NPC별로 생성.</summary>
    public sealed class NpcChatSession : IDisposable
    {
        private readonly LLamaContext _context;
        private readonly ChatSession _session;
        private readonly InferenceParams _inferParams;

        internal NpcChatSession(LLamaWeights model, ModelParams modelParams, string systemPrompt)
        {
            _context = model.CreateContext(modelParams);
            var executor = new InteractiveExecutor(_context);

            var history = new ChatHistory();
            history.AddMessage(AuthorRole.System, systemPrompt);
            _session = new ChatSession(executor, history);

            _inferParams = new InferenceParams
            {
                MaxTokens = 256,
                AntiPrompts = new List<string>(),
                SamplingPipeline = new DefaultSamplingPipeline(),
            };
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            var parts = new List<string>();
            await foreach (var token in _session.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, userMessage),
                               _inferParams))
            {
                parts.Add(token);
            }
            return string.Concat(parts).Trim();
        }

        public void Dispose() => _context.Dispose();
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

        /// <summary>NPC 전용 독립 세션 생성. 모델 가중치를 공유하므로 메모리 효율적.</summary>
        public NpcChatSession CreateNpcSession(string systemPrompt)
            => new NpcChatSession(_model.Model, _model.ModelParams, systemPrompt);

        public void Dispose() => _model.Dispose();
    }
}
