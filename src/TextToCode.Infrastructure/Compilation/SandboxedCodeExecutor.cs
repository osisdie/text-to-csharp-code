using System.Reflection;
using System.Runtime.Loader;
using TextToCode.Core.Entities;
using TextToCode.Core.Interfaces;

namespace TextToCode.Infrastructure.Compilation;

public sealed class SandboxedCodeExecutor : ICodeExecutor
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public async Task<ExecutionResult> ExecuteAsync(byte[] assemblyBytes, CancellationToken cancellationToken = default)
    {
        var context = new CollectibleAssemblyLoadContext();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            Assembly assembly;
            using (var ms = new MemoryStream(assemblyBytes))
            {
                assembly = context.LoadFromStream(ms);
            }

            var entryPoint = assembly.EntryPoint
                ?? throw new InvalidOperationException("Assembly has no entry point. Ensure code has a Main method or top-level statements.");

            var consoleOutput = new StringWriter();
            var originalOut = Console.Out;
            var originalError = Console.Error;

            Console.SetOut(consoleOutput);
            Console.SetError(consoleOutput);

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(DefaultTimeout);

                await Task.Run(() =>
                {
                    var parameters = entryPoint.GetParameters();
                    var args = parameters.Length > 0 ? new object?[] { Array.Empty<string>() } : null;
                    var result = entryPoint.Invoke(null, args);

                    if (result is Task task)
                    {
                        task.GetAwaiter().GetResult();
                    }
                }, cts.Token);

                sw.Stop();
                return ExecutionResult.Succeeded(consoleOutput.ToString(), sw.Elapsed);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                sw.Stop();
                return ExecutionResult.Failed(
                    consoleOutput.ToString(),
                    ex.InnerException.Message,
                    ex.InnerException.GetType().FullName,
                    sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                return ExecutionResult.Failed(
                    consoleOutput.ToString(),
                    $"Execution timed out after {DefaultTimeout.TotalSeconds}s",
                    nameof(OperationCanceledException),
                    sw.Elapsed);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Failed(
                string.Empty,
                ex.Message,
                ex.GetType().FullName,
                sw.Elapsed);
        }
        finally
        {
            context.Unload();
        }
    }

    private sealed class CollectibleAssemblyLoadContext() : AssemblyLoadContext(isCollectible: true)
    {
        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }
}
