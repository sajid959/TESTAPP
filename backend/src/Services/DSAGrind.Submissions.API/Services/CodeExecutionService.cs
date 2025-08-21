using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text;
using System.Text.Json;

namespace DSAGrind.Submissions.API.Services;

public class CodeExecutionService : ICodeExecutionService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<CodeExecutionService> _logger;
    private readonly Dictionary<string, ExecutorConfig> _executorConfigs;

    public CodeExecutionService(ILogger<CodeExecutionService> logger)
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _logger = logger;
        _executorConfigs = InitializeExecutorConfigs();
    }

    public async Task<CodeExecutionResultDto> ExecuteCodeAsync(string problemId, string code, string language, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, fetch problem test cases from Problems API
            var testCases = await GetProblemTestCasesAsync(problemId, cancellationToken);
            
            var results = new List<TestResultDto>();
            var totalRuntime = 0;
            var totalMemory = 0;

            foreach (var (testCase, index) in testCases.Select((tc, i) => (tc, i)))
            {
                var result = await ExecuteCodeForTestCaseAsync(code, language, testCase.Input, testCase.ExpectedOutput, cancellationToken);
                
                results.Add(new TestResultDto
                {
                    TestCaseIndex = index,
                    Passed = result.Status == "success" && result.Output?.Trim() == testCase.ExpectedOutput.Trim(),
                    Input = testCase.Input,
                    ExpectedOutput = testCase.ExpectedOutput,
                    ActualOutput = result.Output ?? "",
                    Runtime = result.Runtime,
                    Memory = result.Memory,
                    ErrorMessage = result.ErrorMessage
                });

                totalRuntime += result.Runtime;
                totalMemory = Math.Max(totalMemory, result.Memory);
            }

            var allPassed = results.All(r => r.Passed);
            var status = allPassed ? "accepted" : "wrong_answer";

            return new CodeExecutionResultDto
            {
                Status = status,
                Runtime = totalRuntime / testCases.Count, // Average runtime
                Memory = totalMemory,
                TestResults = results,
                ExecutionDetails = new ExecutionDetailsDto
                {
                    TotalRuntime = totalRuntime,
                    PeakMemoryUsage = totalMemory,
                    ExecutorVersion = "docker-v1.0",
                    SandboxInfo = new SandboxInfoDto
                    {
                        ExecutionStartTime = DateTime.UtcNow,
                        ExecutionEndTime = DateTime.UtcNow.AddMilliseconds(totalRuntime),
                        ResourceLimits = new ResourceLimitsDto
                        {
                            TimeLimit = 1000,
                            MemoryLimit = 256,
                            CpuLimit = 1.0
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing code for problem {ProblemId}", problemId);
            return new CodeExecutionResultDto
            {
                Status = "error",
                ErrorMessage = "Execution failed: " + ex.Message,
                ExecutionDetails = new ExecutionDetailsDto()
            };
        }
    }

    public async Task<CodeExecutionResultDto> TestCodeAsync(string code, string language, string input, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteCodeForTestCaseAsync(code, language, input, "", cancellationToken);
            
            return new CodeExecutionResultDto
            {
                Status = result.Status,
                Output = result.Output,
                ErrorMessage = result.ErrorMessage,
                Runtime = result.Runtime,
                Memory = result.Memory,
                ExecutionDetails = new ExecutionDetailsDto
                {
                    TotalRuntime = result.Runtime,
                    PeakMemoryUsage = result.Memory,
                    ExecutorVersion = "docker-v1.0"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing code");
            return new CodeExecutionResultDto
            {
                Status = "error",
                ErrorMessage = "Test execution failed: " + ex.Message,
                ExecutionDetails = new ExecutionDetailsDto()
            };
        }
    }

    public async Task<List<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_executorConfigs.Keys.ToList());
    }

    private async Task<ExecutionResult> ExecuteCodeForTestCaseAsync(string code, string language, string input, string expectedOutput, CancellationToken cancellationToken)
    {
        if (!_executorConfigs.TryGetValue(language.ToLower(), out var config))
        {
            return new ExecutionResult
            {
                Status = "error",
                ErrorMessage = $"Unsupported language: {language}"
            };
        }

        var startTime = DateTime.UtcNow;
        
        try
        {
            // Create container
            var createResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = config.Image,
                Cmd = config.GetExecutionCommand(code),
                NetworkDisabled = true,
                HostConfig = new HostConfig
                {
                    Memory = 256 * 1024 * 1024, // 256 MB
                    CPUQuota = 100000, // 1 CPU
                    AutoRemove = true
                },
                WorkingDir = "/app"
            }, cancellationToken);

            // Start container
            await _dockerClient.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters(), cancellationToken);

            // Send input if provided
            if (!string.IsNullOrEmpty(input))
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                using var inputStream = new MemoryStream(inputBytes);
                await _dockerClient.Containers.AttachContainerAsync(createResponse.ID, false, 
                    new ContainerAttachParameters
                    {
                        Stream = true,
                        Stdin = true
                    }, inputStream, cancellationToken);
            }

            // Wait for container to finish
            var waitResponse = await _dockerClient.Containers.WaitContainerAsync(createResponse.ID, cancellationToken);

            // Get logs
            var logsStream = await _dockerClient.Containers.GetContainerLogsAsync(createResponse.ID,
                new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true
                }, cancellationToken);

            using var reader = new StreamReader(logsStream);
            var output = await reader.ReadToEndAsync();

            var runtime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ExecutionResult
            {
                Status = waitResponse.StatusCode == 0 ? "success" : "error",
                Output = output.Trim(),
                ErrorMessage = waitResponse.StatusCode != 0 ? "Runtime error" : null,
                Runtime = runtime,
                Memory = 64 // Simplified memory tracking
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker execution failed");
            return new ExecutionResult
            {
                Status = "error",
                ErrorMessage = ex.Message,
                Runtime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    private async Task<List<(string Input, string ExpectedOutput)>> GetProblemTestCasesAsync(string problemId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would call the Problems API
        // For now, return sample test cases
        return await Task.FromResult(new List<(string, string)>
        {
            ("5", "5"),
            ("10", "10"),
            ("0", "0")
        });
    }

    private Dictionary<string, ExecutorConfig> InitializeExecutorConfigs()
    {
        return new Dictionary<string, ExecutorConfig>
        {
            ["python"] = new ExecutorConfig
            {
                Image = "python:3.9-slim",
                FileExtension = ".py",
                CompileCommand = null,
                ExecuteCommand = "python /app/solution.py"
            },
            ["javascript"] = new ExecutorConfig
            {
                Image = "node:18-slim",
                FileExtension = ".js", 
                CompileCommand = null,
                ExecuteCommand = "node /app/solution.js"
            },
            ["java"] = new ExecutorConfig
            {
                Image = "openjdk:11-slim",
                FileExtension = ".java",
                CompileCommand = "javac /app/Solution.java",
                ExecuteCommand = "java -cp /app Solution"
            },
            ["cpp"] = new ExecutorConfig
            {
                Image = "gcc:latest",
                FileExtension = ".cpp",
                CompileCommand = "g++ -o /app/solution /app/solution.cpp",
                ExecuteCommand = "/app/solution"
            },
            ["csharp"] = new ExecutorConfig
            {
                Image = "mcr.microsoft.com/dotnet/sdk:8.0",
                FileExtension = ".cs",
                CompileCommand = "dotnet build /app",
                ExecuteCommand = "dotnet run --project /app"
            }
        };
    }

    private class ExecutorConfig
    {
        public string Image { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public string? CompileCommand { get; set; }
        public string ExecuteCommand { get; set; } = "";

        public List<string> GetExecutionCommand(string code)
        {
            var commands = new List<string> { "/bin/sh", "-c" };
            
            var script = $"echo '{code.Replace("'", "'\\''")}' > /app/solution{FileExtension}";
            
            if (!string.IsNullOrEmpty(CompileCommand))
            {
                script += $" && {CompileCommand}";
            }
            
            script += $" && {ExecuteCommand}";
            
            commands.Add(script);
            return commands;
        }
    }

    private class ExecutionResult
    {
        public string Status { get; set; } = "";
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public int Runtime { get; set; }
        public int Memory { get; set; }
    }
}