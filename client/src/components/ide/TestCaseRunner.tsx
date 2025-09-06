import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Play, Clock, Memory, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { AITestCaseGenerator, TestCase, TestCaseResult } from '@/services/aiTestCaseGenerator';

interface TestCaseRunnerProps {
  code: string;
  language: string;
  problemId: string;
  onRunTests: () => void;
}

export function TestCaseRunner({ code, language, problemId, onRunTests }: TestCaseRunnerProps) {
  const [testCases, setTestCases] = useState<TestCase[]>([]);
  const [results, setResults] = useState<TestCaseResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);
  const [runProgress, setRunProgress] = useState(0);
  const [selectedTab, setSelectedTab] = useState('visible');

  // Load test cases when component mounts
  useEffect(() => {
    loadTestCases();
  }, [problemId]);

  const loadTestCases = async () => {
    try {
      // In a real implementation, this would fetch from the backend
      // For now, we'll generate default test cases
      const mockTestCases: TestCase[] = [
        {
          id: 'example-1',
          input: 'nums = [2,7,11,15], target = 9',
          expectedOutput: '[0,1]',
          isHidden: false,
          timeLimit: 1000,
          memoryLimit: 256
        },
        {
          id: 'example-2',
          input: 'nums = [3,2,4], target = 6',
          expectedOutput: '[1,2]',
          isHidden: false,
          timeLimit: 1000,
          memoryLimit: 256
        },
        {
          id: 'edge-1',
          input: 'nums = [3,3], target = 6',
          expectedOutput: '[0,1]',
          isHidden: true,
          timeLimit: 1000,
          memoryLimit: 256
        },
        {
          id: 'edge-2',
          input: 'nums = [], target = 0',
          expectedOutput: '[]',
          isHidden: true,
          timeLimit: 1000,
          memoryLimit: 256
        },
        {
          id: 'stress-1',
          input: 'nums = [' + Array(1000).fill(0).map((_, i) => i).join(',') + '], target = 1998',
          expectedOutput: '[999,999]',
          isHidden: true,
          timeLimit: 2000,
          memoryLimit: 512
        }
      ];
      
      setTestCases(mockTestCases);
    } catch (error) {
      console.error('Error loading test cases:', error);
    }
  };

  const runTests = async () => {
    if (!code.trim()) {
      return;
    }

    setIsRunning(true);
    setRunProgress(0);
    setResults([]);
    onRunTests();

    try {
      const testResults = await AITestCaseGenerator.executeTestCases(code, language, testCases);
      
      // Simulate progress updates
      for (let i = 0; i <= testCases.length; i++) {
        setRunProgress((i / testCases.length) * 100);
        if (i < testResults.length) {
          setResults(prev => [...prev, testResults[i]]);
        }
        await new Promise(resolve => setTimeout(resolve, 200));
      }
      
      setResults(testResults);
    } catch (error) {
      console.error('Error running tests:', error);
    } finally {
      setIsRunning(false);
      setRunProgress(100);
    }
  };

  const visibleTestCases = testCases.filter(tc => !tc.isHidden);
  const hiddenTestCases = testCases.filter(tc => tc.isHidden);
  const visibleResults = results.filter((_, index) => !testCases[index]?.isHidden);
  const hiddenResults = results.filter((_, index) => testCases[index]?.isHidden);

  const passedCount = results.filter(r => r.passed).length;
  const totalCount = results.length;
  const allPassed = totalCount > 0 && passedCount === totalCount;

  return (
    <div className="space-y-4">
      {/* Header with Run Tests button */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Test Cases</h3>
        <Button 
          onClick={runTests} 
          disabled={isRunning || !code.trim()}
          className="flex items-center gap-2"
        >
          <Play className="w-4 h-4" />
          {isRunning ? 'Running...' : 'Run Tests'}
        </Button>
      </div>

      {/* Progress bar when running */}
      {isRunning && (
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-2">
              <div className="flex justify-between text-sm text-gray-600">
                <span>Running test cases...</span>
                <span>{Math.round(runProgress)}%</span>
              </div>
              <Progress value={runProgress} className="w-full" />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Results summary */}
      {results.length > 0 && (
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                {allPassed ? (
                  <CheckCircle className="w-5 h-5 text-green-600" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-600" />
                )}
                <span className="font-medium">
                  {passedCount}/{totalCount} test cases passed
                </span>
              </div>
              <Badge variant={allPassed ? 'default' : 'destructive'}>
                {allPassed ? 'All Passed' : 'Some Failed'}
              </Badge>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Test cases and results */}
      {testCases.length > 0 ? (
        <Tabs value={selectedTab} onValueChange={setSelectedTab}>
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="visible">
              Visible Tests ({visibleTestCases.length})
            </TabsTrigger>
            <TabsTrigger value="hidden" disabled={!results.length}>
              Hidden Tests ({hiddenTestCases.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="visible" className="space-y-3">
            {visibleTestCases.length > 0 ? (
              visibleTestCases.map((testCase, index) => (
                <TestCaseCard 
                  key={testCase.id}
                  testCase={testCase}
                  result={visibleResults[index]}
                  showDetails={true}
                />
              ))
            ) : (
              <Card>
                <CardContent className="pt-6 text-center text-gray-500">
                  No visible test cases available
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="hidden" className="space-y-3">
            {hiddenTestCases.length > 0 ? (
              hiddenTestCases.map((testCase, visibleIndex) => {
                const actualIndex = testCases.findIndex(tc => tc.id === testCase.id);
                return (
                  <TestCaseCard 
                    key={testCase.id}
                    testCase={testCase}
                    result={results[actualIndex]}
                    showDetails={false}
                  />
                );
              })
            ) : (
              <Card>
                <CardContent className="pt-6 text-center text-gray-500">
                  No hidden test cases available
                </CardContent>
              </Card>
            )}
          </TabsContent>
        </Tabs>
      ) : (
        <Card>
          <CardContent className="pt-6">
            <div className="text-center text-gray-500 flex items-center justify-center gap-2">
              <AlertCircle className="w-5 h-5" />
              No test cases available
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

interface TestCaseCardProps {
  testCase: TestCase;
  result?: TestCaseResult;
  showDetails: boolean;
}

function TestCaseCard({ testCase, result, showDetails }: TestCaseCardProps) {
  const getStatusColor = (status?: string) => {
    switch (status) {
      case 'PASSED': return 'text-green-600';
      case 'FAILED': return 'text-red-600';
      case 'TIME_LIMIT_EXCEEDED': return 'text-orange-600';
      case 'MEMORY_LIMIT_EXCEEDED': return 'text-purple-600';
      case 'RUNTIME_ERROR': return 'text-red-600';
      default: return 'text-gray-600';
    }
  };

  const getStatusIcon = (status?: string) => {
    switch (status) {
      case 'PASSED': return <CheckCircle className="w-4 h-4 text-green-600" />;
      case 'FAILED': return <XCircle className="w-4 h-4 text-red-600" />;
      case 'TIME_LIMIT_EXCEEDED': return <Clock className="w-4 h-4 text-orange-600" />;
      case 'MEMORY_LIMIT_EXCEEDED': return <Memory className="w-4 h-4 text-purple-600" />;
      case 'RUNTIME_ERROR': return <AlertCircle className="w-4 h-4 text-red-600" />;
      default: return null;
    }
  };

  return (
    <Card className={`border-l-4 ${result?.passed ? 'border-l-green-500' : result ? 'border-l-red-500' : 'border-l-gray-300'}`}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            {result && getStatusIcon(result.status)}
            <span className="font-medium">Test Case</span>
            {testCase.isHidden && (
              <Badge variant="secondary" className="text-xs">Hidden</Badge>
            )}
          </div>
          {result && (
            <div className="flex items-center gap-4 text-sm text-gray-600">
              <div className="flex items-center gap-1">
                <Clock className="w-3 h-3" />
                {result.executionTime.toFixed(0)}ms
              </div>
              <div className="flex items-center gap-1">
                <Memory className="w-3 h-3" />
                {result.memoryUsed.toFixed(1)}MB
              </div>
            </div>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {showDetails ? (
          <>
            <div>
              <label className="text-sm font-medium text-gray-700">Input:</label>
              <ScrollArea className="h-auto max-h-20">
                <code className="block text-sm bg-gray-100 dark:bg-gray-800 p-2 rounded mt-1">
                  {testCase.input}
                </code>
              </ScrollArea>
            </div>
            
            {result && (
              <>
                <div>
                  <label className="text-sm font-medium text-gray-700">Expected Output:</label>
                  <code className="block text-sm bg-gray-100 dark:bg-gray-800 p-2 rounded mt-1">
                    {testCase.expectedOutput}
                  </code>
                </div>
                
                <div>
                  <label className="text-sm font-medium text-gray-700">Your Output:</label>
                  <code className={`block text-sm p-2 rounded mt-1 ${
                    result.passed ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-800'
                  }`}>
                    {result.actualOutput || 'No output'}
                  </code>
                </div>

                {result.error && (
                  <div>
                    <label className="text-sm font-medium text-red-700">Error:</label>
                    <code className="block text-sm bg-red-50 text-red-800 p-2 rounded mt-1">
                      {result.error}
                    </code>
                  </div>
                )}
              </>
            )}
          </>
        ) : (
          <div className="text-center py-4">
            {result ? (
              <div className="space-y-2">
                <div className={`text-lg font-medium ${getStatusColor(result.status)}`}>
                  {result.status.replace(/_/g, ' ')}
                </div>
                {result.error && (
                  <p className="text-sm text-red-600">{result.error}</p>
                )}
              </div>
            ) : (
              <p className="text-gray-500">Hidden test case - run tests to see results</p>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}