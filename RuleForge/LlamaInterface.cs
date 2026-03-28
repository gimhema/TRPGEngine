
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace RuleForge
{
    /// <summary>
    /// llama.cpp 네이티브 라이브러리가 fd 2(stderr)에 직접 쓰는 출력을 억제한다.
    /// managed 로그 콜백을 우회하는 네이티브 출력도 차단된다.
    /// </summary>
    internal static class NativeStderrSuppressor
    {
        [DllImport("libc", EntryPoint = "dup")]
        private static extern int Dup(int fd);

        [DllImport("libc", EntryPoint = "dup2")]
        private static extern int Dup2(int oldfd, int newfd);

        [DllImport("libc", EntryPoint = "open")]
        private static extern int Open(string path, int flags);

        private const int O_WRONLY = 1;
        private const int STDERR_FD = 2;
        private static int _savedFd = -1;

        /// <summary>stderr(fd 2)를 /dev/null로 리다이렉트한다. 복원 전까지 네이티브 출력이 차단된다.</summary>
        public static void Suppress()
        {
            if (!OperatingSystem.IsLinux() || _savedFd >= 0) return;
            _savedFd = Dup(STDERR_FD);
            int devNull = Open("/dev/null", O_WRONLY);
            if (devNull >= 0) Dup2(devNull, STDERR_FD);
        }

        /// <summary>stderr를 원래 fd로 복원한다.</summary>
        public static void Restore()
        {
            if (!OperatingSystem.IsLinux() || _savedFd < 0) return;
            Dup2(_savedFd, STDERR_FD);
            _savedFd = -1;
        }
    }

    /// <summary>LLM 작업 중 콘솔 스피너를 표시하는 유틸리티.</summary>
    public static class ConsoleSpinner
    {
        private static readonly string[] Frames = ["|", "/", "-", "\\"];

        /// <summary>
        /// message를 표시하면서 work를 실행한다. 완료 후 해당 줄을 지운다.
        /// </summary>
        public static T Run<T>(string message, Func<T> work)
        {
            Console.Write($"\r{message} ");
            T result = default!;
            Exception? error = null;

            var thread = new Thread(() =>
            {
                try { result = work(); }
                catch (Exception ex) { error = ex; }
            }) { IsBackground = true };
            thread.Start();

            int frame = 0;
            while (thread.IsAlive)
            {
                Console.Write(Frames[frame++ % Frames.Length]);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Thread.Sleep(100);
            }
            thread.Join();

            // 스피너 줄 지우기
            Console.Write($"\r{new string(' ', message.Length + 4)}\r");

            if (error != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();

            return result;
        }

        /// <summary>반환값 없는 버전.</summary>
        public static void Run(string message, Action work)
            => Run<int>(message, () => { work(); return 0; });
    }
}

namespace RuleForge
{
    /// <summary>
    /// 모델 가중치와 파라미터만 보유한다.
    /// LLamaContext는 세션별로 개별 생성하므로 여기서는 만들지 않는다.
    /// </summary>
    public sealed class ModelDescription : IDisposable
    {
        public string ModelPath { get; }
        public ModelParams ModelParams { get; }
        public LLamaWeights Model { get; }

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
        }

        public void Dispose()
        {
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

        /// <summary>NPC/나레이터 전용 독립 세션 생성. 모델 가중치를 공유하므로 메모리 효율적.</summary>
        public NpcChatSession CreateNpcSession(string systemPrompt)
            => new NpcChatSession(_model.Model, _model.ModelParams, systemPrompt);

        public void Dispose() => _model.Dispose();
    }
}
